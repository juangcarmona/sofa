using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using JuanGCarmona.Lighthouse.Cqrs;
using JuanGCarmona.SoFa.Modules.Workspace.Identity;

namespace JuanGCarmona.SoFa.Modules.Workspace.Source;

internal sealed record GetRepositoryConfiguration(OwnerSession Session, string Repository);
internal sealed record RepositoryConfigurationSource(string Path, string Revision, DateTimeOffset ObservedAt,
    string Availability, string Interpretation, IReadOnlyList<string> Diagnostics, string? SourceUrl = null);
internal sealed record RepositoryConfigurationFact(string Id, string Component, string FactKind, string RawValue,
    string SourcePath, string Revision, DateTimeOffset ObservedAt, string ContractIdentity,
    string Interpretation, string SourceUrl, string? RelatedWorkUrl = null);
internal sealed record RepositoryConfigurationProjection(string Repository, string ConfiguredBranch,
    string? ObservedRevision, string? DataScope, DateTimeOffset ObservedAt, string Availability,
    string Coverage, string Freshness, object Limits, IReadOnlyList<RepositoryConfigurationSource> Sources,
    IReadOnlyList<RepositoryConfigurationFact> Facts, IReadOnlyList<string> Reasons);

internal sealed class GetRepositoryConfigurationHandler(WorkspaceOptions settings, OwnerSessions sessions,
    GitHubAuthorization authorization, RepositoryAccess access, GitHubTransport transport, TimeProvider clock)
    : IQueryHandler<GetRepositoryConfiguration, RepositoryConfigurationProjection>
{
  private const int MaximumFileBytes = 256 * 1024;
  private const int MaximumAggregateBytes = 1024 * 1024;
  private static readonly string[] FixedPaths = ["lighthouse.json", ".lighthouse/upgrade.json", ".lighthouse/sdlc.json",
      ".lighthouse/commands.json", "package.json", "package-lock.json"];
  private sealed record CacheEntry(RepositoryConfigurationProjection Projection, DateTimeOffset StoredAt, int Bytes);
  private readonly object cacheSync = new();
  private readonly Dictionary<string, CacheEntry> cache = new(StringComparer.Ordinal);

  public async Task<RepositoryConfigurationProjection> Handle(GetRepositoryConfiguration query, CancellationToken cancellationToken)
  {
    var observed = clock.GetUtcNow();
    var branch = settings.RepositoryBranches.FirstOrDefault(entry => entry.Key.Equals(query.Repository, StringComparison.OrdinalIgnoreCase)).Value ?? "";
    RepositoryConfigurationProjection Failure(string availability, string reason, string? scope = null) =>
        new(query.Repository, branch, null, scope, clock.GetUtcNow(), availability, "unknown", "unknown",
            new
            {
              maximumContentReads = 8,
              maximumConcurrentReads = 2,
              maximumFileBytes = MaximumFileBytes,
              maximumAggregateBytes = MaximumAggregateBytes,
              deadlineSeconds = 30
            }, [], [], [reason]);

    if (!settings.Repositories.Contains(query.Repository, StringComparer.OrdinalIgnoreCase)
        || !Regex.IsMatch(query.Repository, @"^[A-Za-z0-9_.-]{1,100}/[A-Za-z0-9_.-]{1,100}$", RegexOptions.CultureInvariant)
        || branch.Length is 0 or > 255 || branch.Any(char.IsControl))
      return Failure("invalid", "Choose a configured repository with a configured branch.");

    var record = query.Session;
    var authority = await access.CheckControlsAsync(record, query.Repository, cancellationToken).ConfigureAwait(false);
    if (authority.Availability == SourceAvailability.Denied)
    {
      ClearRepository(record.Id, query.Repository);
      return Failure("denied", "The configured repository is not currently readable.");
    }
    if (authority.Availability != SourceAvailability.Available || authority.Controls is not { } controls)
      return Failure("unavailable", "The connected GitHub App installation and repository identity could not be verified.");
    if (controls.ContentsRead != true)
    {
      ClearRepository(record.Id, query.Repository);
      return Failure(controls.ContentsRead == false ? "denied" : "unavailable",
          "The verified GitHub App installation does not provide contents-read authority.", controls.DataScope);
    }

    var connection = await authorization.CurrentAsync(record, cancellationToken).ConfigureAwait(false);
    if (connection is null) return Failure("disconnected", "Connect and verify the acting GitHub user.");
    long generation;
    lock (record.Sync) generation = record.GitHub.Generation;
    bool Live()
    {
      lock (record.Sync) return ReferenceEquals(sessions.Get(record.Id), record)
          && ReferenceEquals(record.GitHub.Current, connection) && record.GitHub.Generation == generation
          && record.GitHub.Generation == controls.Generation;
    }
    async Task<RepositoryConfigurationProjection?> RetainStale(string reason, CancellationToken token)
    {
      var prefix = record.Id + "\n" + generation.ToString(System.Globalization.CultureInfo.InvariantCulture) + "\n"
          + connection.DataScope + "\n" + query.Repository.ToLowerInvariant() + "\n";
      CacheEntry? previous;
      lock (cacheSync)
        previous = cache.Where(pair => pair.Key.StartsWith(prefix, StringComparison.Ordinal)).Select(pair => pair.Value)
            .OrderByDescending(entry => entry.StoredAt).FirstOrDefault();
      if (previous is null || !Live()) return null;
      var verified = await access.CheckControlsAsync(record, query.Repository, token).ConfigureAwait(false);
      if (!Live() || verified.Availability != SourceAvailability.Available || verified.Controls != controls)
      {
        ClearRepository(record.Id, query.Repository);
        return null;
      }
      return previous.Projection with { Freshness = "stale", Reasons = previous.Projection.Reasons.Append(reason).Distinct().ToArray() };
    }
    var guard = new SourceRequestGuard(record.Sync, Live, connection.Stopped.Token);
    using var budget = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, connection.Stopped.Token);
    budget.CancelAfter(TimeSpan.FromSeconds(30));
    try
    {
      using var resolved = await transport.GetAsync("/repos/" + query.Repository + "/commits/" + Uri.EscapeDataString(branch),
          connection.Tokens.Access, guard, budget.Token).ConfigureAwait(false);
      if (!Live()) return Failure("disconnected", "The source account or session changed.", connection.DataScope);
      if (resolved.Status == HttpStatusCode.Unauthorized)
      {
        GitHubAuthorization.Invalidate(record, connection);
        return Failure("disconnected", "GitHub rejected the source credential.", connection.DataScope);
      }
      if (!resolved.Successful || SourceJson.Text(resolved.Value, "sha") is not { Length: 40 } revision
          || !revision.All(Uri.IsHexDigit))
      {
        if (resolved.Status is not (HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden))
          if (await RetainStale("The configured branch could not be refreshed; the earlier commit and observation time are retained.", budget.Token).ConfigureAwait(false) is { } stale)
            return stale;
        if (resolved.Status == HttpStatusCode.Forbidden) ClearRepository(record.Id, query.Repository);
        if (resolved.Status == HttpStatusCode.Unauthorized) GitHubAuthorization.Invalidate(record, connection);
        return Failure(resolved.Status is HttpStatusCode.Forbidden or HttpStatusCode.NotFound ? "denied" : "unavailable",
            "The configured branch could not be resolved to one immutable commit.", connection.DataScope);
      }

      var cacheKey = record.Id + "\n" + generation.ToString(System.Globalization.CultureInfo.InvariantCulture) + "\n"
          + connection.DataScope + "\n" + query.Repository.ToLowerInvariant() + "\n" + revision.ToLowerInvariant();
      CacheEntry? cached;
      lock (cacheSync) cache.TryGetValue(cacheKey, out cached);
      if (cached is not null && clock.GetUtcNow() - cached.StoredAt <= TimeSpan.FromSeconds(15))
      {
        var cachedAuthority = await access.CheckControlsAsync(record, query.Repository, budget.Token).ConfigureAwait(false);
        if (!Live() || cachedAuthority.Availability != SourceAvailability.Available || cachedAuthority.Controls != controls)
        {
          ClearRepository(record.Id, query.Repository);
          return Failure("disconnected", "The source authority changed before cached configuration could be reused.", connection.DataScope);
        }
        lock (record.Sync)
          if (!Live()) return Failure("disconnected", "The source account or session changed.", connection.DataScope);
        return cached.Projection;
      }

      var sources = new List<RepositoryConfigurationSource>();
      var facts = new List<RepositoryConfigurationFact>();
      var reasons = new List<string>();
      var totalBytes = 0;
      var reads = 0;
      var paths = FixedPaths.ToList();
      for (var pathIndex = 0; pathIndex < paths.Count; pathIndex++)
      {
        var path = paths[pathIndex];
        if (reads >= 8)
        {
          reasons.Add("The bounded configuration read limit was reached; remaining sources are unknown.");
          break;
        }
        if (totalBytes >= MaximumAggregateBytes)
        {
          reasons.Add("The aggregate decoded content read-size limit was reached; remaining sources are unknown.");
          break;
        }
        reads++;
        var sourceTime = clock.GetUtcNow();
        var relative = "contents/" + path + "?ref=" + revision.ToLowerInvariant();
        using var response = await transport.GetAsync("/repos/" + query.Repository + "/" + relative,
            connection.Tokens.Access, guard, budget.Token).ConfigureAwait(false);
        if (!Live()) return Failure("disconnected", "The source account or session changed.", connection.DataScope);
        if (response.Status == HttpStatusCode.Unauthorized)
        {
          ClearRepository(record.Id, query.Repository);
          GitHubAuthorization.Invalidate(record, connection);
          return Failure("disconnected", "GitHub rejected the source credential.", connection.DataScope);
        }
        if (response.Status == HttpStatusCode.Forbidden)
        {
          ClearRepository(record.Id, query.Repository);
          return Failure("denied", "The current connected-user contents read was denied; collected facts were cleared.", connection.DataScope);
        }
        if (!response.Successful)
        {
          if (response.Status is not HttpStatusCode.NotFound)
            if (await RetainStale("A source read failed during refresh; the earlier commit and observation time are retained.", budget.Token).ConfigureAwait(false) is { } stale)
              return stale;
          var state = response.Status == HttpStatusCode.Forbidden ? "denied" : response.Status == HttpStatusCode.NotFound ? "unavailable" : "unavailable";
          sources.Add(new(path, revision, sourceTime, state, "unsupported", ["The file was not readable; absence is not inferred."]));
          continue;
        }
        if (SourceJson.Text(response.Value, "type") != "file" || SourceJson.Text(response.Value, "encoding") != "base64"
            || SourceJson.Text(response.Value, "content") is not { } encoded)
        {
          sources.Add(new(path, revision, sourceTime, "unavailable", "unsupported", ["The source did not return a regular base64 file."]));
          continue;
        }
        byte[] bytes;
        try { bytes = Convert.FromBase64String(encoded); }
        catch (FormatException)
        {
          sources.Add(new(path, revision, sourceTime, "unavailable", "unsupported", ["The source content was malformed base64."]));
          continue;
        }
        if (bytes.Length > MaximumFileBytes)
        {
          sources.Add(new(path, revision, sourceTime, "unavailable", "unsupported", ["The source exceeded the decoded per-file limit; it was not truncated."]));
          reasons.Add("One or more configuration sources exceeded the per-file read-size limit.");
          continue;
        }
        if (totalBytes + bytes.Length > MaximumAggregateBytes)
        {
          sources.Add(new(path, revision, sourceTime, "unavailable", "unsupported", ["The aggregate decoded content limit was reached; this and remaining sources were not read."]));
          reasons.Add("The aggregate decoded content read-size limit was reached; remaining sources are unknown.");
          break;
        }
        totalBytes += bytes.Length;
        if (path.StartsWith(".github/workflows/", StringComparison.Ordinal))
        {
          var digest = Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();
          sources.Add(new(path, revision, sourceTime, "available", "unsupported",
              ["Workflow syntax and embedded tool versions are retained as inert source evidence only."]));
          facts.Add(new(RepositoryConfigurationProjector.FactId(query.Repository, revision, path, "workflow-source"), "workflow", "source-evidence-sha256",
              digest, path, revision, sourceTime, "uninterpreted-workflow-source", "unsupported",
              "https://github.com/" + query.Repository + "/blob/" + revision + "/" + path));
          continue;
        }
        if (!RepositoryConfigurationProjector.TryParse(path, bytes, out var sourceFacts, out var diagnostics))
        {
          sources.Add(new(path, revision, sourceTime, "available", "unsupported", diagnostics));
          continue;
        }
        sources.Add(new(path, revision, sourceTime, "available", "raw", diagnostics));
        foreach (var fact in sourceFacts)
          facts.Add(fact with
          {
            Id = RepositoryConfigurationProjector.FactId(query.Repository, revision, path, fact.Id),
            SourcePath = path,
            Revision = revision,
            ObservedAt = sourceTime,
            SourceUrl = "https://github.com/" + query.Repository + "/blob/" + revision + "/" + path,
            RelatedWorkUrl = fact.RelatedWorkUrl is { } reference ? RelatedWorkUrl(query.Repository, reference) : null
          });
        if (path == "lighthouse.json" && paths.Count == FixedPaths.Length)
        {
          try
          {
            using var manifest = JsonDocument.Parse(bytes);
            if (manifest.RootElement.TryGetProperty("managed", out var managed) && managed.ValueKind == JsonValueKind.Array)
            {
              var declaredPaths = managed.EnumerateArray().Select(item => item.GetString()!).ToArray();
              var supportedPaths = declaredPaths.Where(value => Regex.IsMatch(value,
                  @"^\.github/workflows/[A-Za-z0-9_.-]+\.yml$", RegexOptions.CultureInvariant)).Distinct(StringComparer.Ordinal).ToArray();
              if (supportedPaths.Length > 2 || supportedPaths.Length != declaredPaths.Length)
              {
                reasons.Add("Only two managed workflow files with the supported .github/workflows/*.yml path shape can be inspected; other declarations remain unknown.");
                sources[^1] = sources[^1] with
                {
                  Interpretation = "partial",
                  Diagnostics = sources[^1].Diagnostics.Append("Some manifest-managed paths are outside the bounded workflow reader.").ToArray()
                };
              }
              foreach (var item in supportedPaths.Take(2))
                paths.Add(item);
            }
          }
          catch (JsonException) { }
        }
      }
      if (!Live()) return Failure("disconnected", "The source account or session changed.", connection.DataScope);
      var final = await access.CheckControlsAsync(record, query.Repository, budget.Token).ConfigureAwait(false);
      if (!Live() || final.Availability != SourceAvailability.Available || final.Controls != controls)
        return Failure("disconnected", "The source authority changed during configuration inspection; collected facts were cleared.", connection.DataScope);
      var coverage = sources.Count == paths.Count && sources.All(source => source.Availability == "available") && reasons.Count == 0 ? "complete" : "partial";
      if (coverage == "partial") reasons.Add("Some supported files were unavailable or unsupported; missing configuration is not inferred.");
      var linkedSources = sources.Select(source => source with
      {
        SourceUrl = SafeConfigurationPath(source.Path)
            ? "https://github.com/" + query.Repository + "/blob/" + revision + "/" + source.Path
            : null
      }).ToArray();
      var result = new RepositoryConfigurationProjection(query.Repository, branch, revision, connection.DataScope, observed, "available", coverage, "fresh",
          new
          {
            maximumContentReads = 8,
            maximumConcurrentReads = 2,
            maximumFileBytes = MaximumFileBytes,
            maximumAggregateBytes = MaximumAggregateBytes,
            deadlineSeconds = 30,
            reads,
            decodedBytes = totalBytes
          },
          linkedSources, facts, reasons);
      Store(cacheKey, result);
      return result;
    }
    catch (OperationCanceledException)
    {
      return Failure(Live() ? "unavailable" : "disconnected", "The bounded configuration read ended; coverage is unknown.", connection.DataScope);
    }
  }

  private void Store(string key, RepositoryConfigurationProjection value)
  {
    var bytes = JsonSerializer.SerializeToUtf8Bytes(value).Length;
    if (bytes > 2 * 1024 * 1024) return;
    lock (cacheSync)
    {
      cache.Remove(key);
      while (cache.Count >= 2 || cache.Values.Sum(entry => entry.Bytes) + bytes > 4 * 1024 * 1024)
      {
        if (cache.Count == 0) break;
        var oldest = cache.MinBy(pair => pair.Value.StoredAt).Key;
        cache.Remove(oldest);
      }
      cache[key] = new(value, clock.GetUtcNow(), bytes);
    }
  }

  private void ClearRepository(string sessionId, string repository)
  {
    var prefix = sessionId + "\n";
    lock (cacheSync)
      foreach (var key in cache.Keys.Where(key => key.StartsWith(prefix, StringComparison.Ordinal)
          && key.Contains("\n" + repository.ToLowerInvariant() + "\n", StringComparison.Ordinal)).ToArray()) cache.Remove(key);
  }

  private string? RelatedWorkUrl(string sourceRepository, string reference)
  {
    if (reference.Length is 0 or > 2048) return null;
    var match = Regex.Match(reference, @"^#([1-9][0-9]{0,9})$", RegexOptions.CultureInvariant);
    if (match.Success && int.TryParse(match.Groups[1].Value, out var localIssue))
      return "https://github.com/" + sourceRepository + "/issues/" + localIssue.ToString(System.Globalization.CultureInfo.InvariantCulture);
    if (!Uri.TryCreate(reference, UriKind.Absolute, out var uri) || uri.Scheme != Uri.UriSchemeHttps
        || !uri.Host.Equals("github.com", StringComparison.OrdinalIgnoreCase) || uri.UserInfo.Length != 0
        || uri.Query.Length != 0 || uri.Fragment.Length != 0) return null;
    var segments = uri.AbsolutePath.Split('/', StringSplitOptions.RemoveEmptyEntries);
    if (segments.Length != 4 || !segments[2].Equals("issues", StringComparison.Ordinal)
        || !Regex.IsMatch(segments[3], @"^[1-9][0-9]{0,9}$", RegexOptions.CultureInvariant)
        || !int.TryParse(segments[3], out var issue)) return null;
    var target = segments[0] + "/" + segments[1];
    if (!settings.Repositories.Contains(target, StringComparer.OrdinalIgnoreCase)) return null;
    return "https://github.com/" + target + "/issues/" + issue.ToString(System.Globalization.CultureInfo.InvariantCulture);
  }

  private static bool SafeConfigurationPath(string path) => FixedPaths.Contains(path, StringComparer.Ordinal)
      || Regex.IsMatch(path, @"^\.github/workflows/[A-Za-z0-9_.-]+\.yml$", RegexOptions.CultureInvariant);
}

internal static class RepositoryConfigurationProjector
{
  private static readonly HashSet<string> SupportedPackages = new(StringComparer.Ordinal)
  { "@juangcarmona/lighthouse", "@juangcarmona/lighthouse-sdlc", "@juangcarmona/lighthouse-ui", "@juangcarmona/productshape", "@juangcarmona/arcshape" };
  private static readonly string[] ManifestRepositoryLayout = ["src", "docs", "infra", "scripts"];

  internal static string FactId(string repository, string revision, string path, string field) => Convert.ToHexString(
      SHA256.HashData(Encoding.UTF8.GetBytes(repository.ToLowerInvariant() + "\n" + revision + "\n" + path + "\n" + field))).ToLowerInvariant();

  internal static bool TryParse(string path, byte[] bytes, out List<RepositoryConfigurationFact> facts, out List<string> diagnostics)
  {
    facts = [];
    diagnostics = [];
    try
    {
      using var document = JsonDocument.Parse(bytes, new JsonDocumentOptions { MaxDepth = 64, CommentHandling = JsonCommentHandling.Disallow, AllowTrailingCommas = false });
      if (HasDuplicateKeys(bytes)) { diagnostics.Add("The JSON contains duplicate object keys; interpretation is unsupported."); return false; }
      var root = document.RootElement;
      if (root.ValueKind != JsonValueKind.Object) { diagnostics.Add("The JSON root is not an object."); return false; }
      if (path is "package.json" or "package-lock.json") ProjectNpm(path, root, facts, diagnostics);
      else if (path == "lighthouse.json")
      {
        if (!ValidManifest(root, diagnostics)) return false;
        ProjectManifest(root, facts, diagnostics);
      }
      else if (path == ".lighthouse/upgrade.json")
      {
        if (!ValidUpgrade(root, diagnostics)) return false;
        ProjectUpgrade(root, facts, diagnostics);
      }
      else ProjectRawContract(path, root, facts, diagnostics);
      if (facts.Count > 256 || facts.Any(fact => fact.RawValue.Length > 2048 || fact.Component.Length > 256
          || fact.Id.Length > 512 || fact.ContractIdentity.Length > 128))
      {
        facts.Clear();
        diagnostics.Add("The source produced more than 256 facts or an individual fact exceeded its display bound; interpretation is unsupported.");
        return false;
      }
      if (diagnostics.Count > 32)
      {
        diagnostics = diagnostics.Take(31).Append("Additional source diagnostics were omitted at the 32-message display limit.").ToList();
      }
      return true;
    }
    catch (JsonException) { diagnostics.Add("The source is invalid JSON."); return false; }
  }

  private static bool HasDuplicateKeys(ReadOnlySpan<byte> json)
  {
    var reader = new Utf8JsonReader(json, new JsonReaderOptions { MaxDepth = 64 });
    var scopes = new Stack<HashSet<string>>();
    while (reader.Read())
    {
      if (reader.TokenType == JsonTokenType.StartObject) scopes.Push(new(StringComparer.Ordinal));
      else if (reader.TokenType == JsonTokenType.EndObject) scopes.Pop();
      else if (reader.TokenType == JsonTokenType.PropertyName && (scopes.Count == 0 || !scopes.Peek().Add(reader.GetString()!))) return true;
    }
    return false;
  }

  private static void ProjectNpm(string path, JsonElement root, List<RepositoryConfigurationFact> facts, List<string> diagnostics)
  {
    if (path == "package.json")
    {
      foreach (var section in new[] { "dependencies", "devDependencies", "optionalDependencies", "peerDependencies" })
      {
        if (!root.TryGetProperty(section, out var entries)) continue;
        if (entries.ValueKind != JsonValueKind.Object) { diagnostics.Add(section + " is not an object."); continue; }
        foreach (var entry in entries.EnumerateObject().Where(entry => SupportedPackages.Contains(entry.Name)))
          if (entry.Value.ValueKind == JsonValueKind.String)
            facts.Add(new(section + "/" + entry.Name, entry.Name, "declared-package", entry.Value.GetString()!, path, "", default,
                "npm-package-json", "raw", ""));
          else diagnostics.Add(section + "/" + entry.Name + " is not a string declaration.");
      }
      if (!facts.Any(fact => fact.FactKind == "declared-package") && diagnostics.Count == 0)
        facts.Add(new("root-package-scope", "npm", "applicability", "not-applicable within root npm package sections",
            path, "", default, "npm-package-json", "supported-root-scope-only", ""));
      return;
    }
    if (!root.TryGetProperty("lockfileVersion", out var lockVersion) || lockVersion.ValueKind != JsonValueKind.Number
        || !lockVersion.TryGetInt32(out var lockVersionNumber) || lockVersionNumber != 3
        || !root.TryGetProperty("packages", out var packages) || packages.ValueKind != JsonValueKind.Object)
    { diagnostics.Add("Only npm lockfile version 3 with a packages object is supported."); return; }
    foreach (var package in packages.EnumerateObject())
    {
      var marker = package.Name.LastIndexOf("node_modules/", StringComparison.Ordinal);
      var packageName = marker >= 0 ? package.Name[(marker + "node_modules/".Length)..] : package.Name;
      if (package.Value.ValueKind != JsonValueKind.Object || !package.Value.TryGetProperty("version", out var version)
          || version.ValueKind != JsonValueKind.String || !SupportedPackages.Contains(packageName)) continue;
      facts.Add(new("packages/" + package.Name + "/version", packageName, "lock-resolved-package", version.GetString()!, path,
          "", default, "npm-lockfile-v3", "raw", ""));
    }
  }

  private static void ProjectManifest(JsonElement root, List<RepositoryConfigurationFact> facts, List<string> diagnostics)
  {
    AddScalar(root, "schemaVersion", "lighthouse-manifest-v1", "contract-version", facts, diagnostics);
    AddScalar(root, "baseline", "lighthouse", "manifest-baseline", facts, diagnostics);
    AddScalar(root, "product", "lighthouse", "product-name", facts, diagnostics, "lighthouse-manifest-v1");
    if (root.TryGetProperty("profiles", out var profiles))
      foreach (var profile in profiles.EnumerateArray())
        facts.Add(new("profiles/" + profile.GetString(), "lighthouse-profile", "profile", profile.GetString()!, "", "", default, "lighthouse-manifest-v1", "validated", ""));
    if (root.TryGetProperty("managed", out var managed))
      foreach (var path in managed.EnumerateArray().Select(value => value.GetString()!))
        facts.Add(new("managed/" + path, "managed-source", "managed-path", path, "", "", default, "lighthouse-manifest-v1", "validated", ""));
    if (root.TryGetProperty("integrations", out var integrations) && integrations.ValueKind == JsonValueKind.Object)
      foreach (var name in new[] { "productShape", "arcShape" })
        if (integrations.TryGetProperty(name, out var pin)) AddPin("integrations/" + name,
            name == "productShape" ? "@juangcarmona/productshape" : "@juangcarmona/arcshape", pin, "lighthouse-manifest-v1", facts, diagnostics);
    if (root.TryGetProperty("packages", out var packageSet) && packageSet.ValueKind == JsonValueKind.Object)
      foreach (var name in new[] { "sdlc", "ui" })
        if (packageSet.TryGetProperty(name, out var package) && package.ValueKind == JsonValueKind.Object)
        {
          var component = name == "sdlc" ? "@juangcarmona/lighthouse-sdlc" : "@juangcarmona/lighthouse-ui";
          if (package.TryGetProperty("version", out var version)) AddPin("packages/" + name + "/version", component, version, "lighthouse-manifest-v1", facts, diagnostics);
        }
  }

  private static void ProjectUpgrade(JsonElement root, List<RepositoryConfigurationFact> facts, List<string> diagnostics)
  {
    AddScalar(root, "schemaVersion", "lighthouse-upgrade-v1", "contract-version", facts, diagnostics);
    AddScalar(root, "baseline", "lighthouse", "upgrade-baseline", facts, diagnostics);
    if (root.TryGetProperty("package", out var package) && package.ValueKind == JsonValueKind.Object)
      AddScalar(package, "version", "@juangcarmona/lighthouse", "declared-package", facts, diagnostics, "lighthouse-upgrade-v1");
    foreach (var (section, kind) in new[] { ("capabilities", "capability-version"), ("projections", "projection-version") })
      if (root.TryGetProperty(section, out var entries) && entries.ValueKind == JsonValueKind.Object)
        foreach (var entry in entries.EnumerateObject())
          facts.Add(new(section + "/" + entry.Name, entry.Name, kind, entry.Value.ValueKind == JsonValueKind.Null ? "null" : entry.Value.GetString()!, "", "", default, "lighthouse-upgrade-v1", "validated", ""));
    if (root.TryGetProperty("adoption", out var adoption))
      foreach (var item in adoption.EnumerateObject())
      {
        var value = item.Value;
        var id = SourceJson.Text(value, "id") ?? item.Name;
        foreach (var property in new[] { "status", "selection", "provider", "version", "catalogVersion", "nextAction", "recordPath" })
          if (value.TryGetProperty(property, out var field) && field.ValueKind is JsonValueKind.String or JsonValueKind.Null)
          {
            var raw = field.ValueKind == JsonValueKind.Null ? "unknown" : field.GetString()!;
            facts.Add(new("adoption/" + item.Name + "/" + property, id, "adoption-" + property, raw, "", "", default,
                "lighthouse-upgrade-v1", "validated", "", property == "nextAction" ? raw : null));
          }
      }
  }

  private static bool ValidManifest(JsonElement root, List<string> diagnostics)
  {
    var allowed = new HashSet<string>(["$schema", "schemaVersion", "baseline", "product", "application", "profiles", "integrations", "packages", "managed", "repository"], StringComparer.Ordinal);
    if (!ClosedObject(root, allowed, ["schemaVersion", "baseline", "product", "profiles", "integrations", "managed"], diagnostics, "manifest")) return false;
    if (root.GetProperty("schemaVersion").ValueKind != JsonValueKind.Number || !root.GetProperty("schemaVersion").TryGetInt32(out var schema) || schema != 1
        || root.GetProperty("baseline").ValueKind != JsonValueKind.String || root.GetProperty("baseline").GetString() != "0.1.0"
        || root.GetProperty("product").ValueKind != JsonValueKind.String
        || !Regex.IsMatch(root.GetProperty("product").GetString()!, @"^[A-Za-z0-9][A-Za-z0-9 ._-]{0,63}$", RegexOptions.CultureInvariant))
    { diagnostics.Add("The Lighthouse manifest does not match the released 0.1.0 schema; raw interpretation is unsupported."); return false; }
    if (root.TryGetProperty("$schema", out var schemaUri) && schemaUri.ValueKind != JsonValueKind.String)
    { diagnostics.Add("The manifest schema reference is not a string."); return false; }
    if (root.TryGetProperty("application", out var application))
    {
      if (!ClosedObject(application, ["shape", "stacks"], ["shape", "stacks"], diagnostics, "manifest application")) return false;
      var shapes = new HashSet<string>(["unknown", "frontend", "backend", "full-stack", "service", "library", "tooling", "cli", "worker", "mobile"], StringComparer.Ordinal);
      var shape = application.GetProperty("shape"); var stacks = application.GetProperty("stacks");
      if (shape.ValueKind != JsonValueKind.String || !shapes.Contains(shape.GetString()!) || stacks.ValueKind != JsonValueKind.Array
          || stacks.EnumerateArray().Any(value => value.ValueKind != JsonValueKind.String || string.IsNullOrWhiteSpace(value.GetString()))
          || stacks.EnumerateArray().Select(value => value.GetString()).Distinct(StringComparer.Ordinal).Count() != stacks.GetArrayLength())
      { diagnostics.Add("Manifest application fields do not match the released schema."); return false; }
    }
    var profiles = root.GetProperty("profiles");
    var supportedProfiles = new HashSet<string>(["node-tooling", "dotnet-backend", "visual-theme"], StringComparer.Ordinal);
    if (profiles.ValueKind != JsonValueKind.Array || profiles.EnumerateArray().Any(value => value.ValueKind != JsonValueKind.String || !supportedProfiles.Contains(value.GetString()!))
        || profiles.EnumerateArray().Select(value => value.GetString()).Distinct(StringComparer.Ordinal).Count() != profiles.GetArrayLength())
    { diagnostics.Add("Manifest profiles are unknown, malformed or duplicated; interpretation is unsupported."); return false; }
    var integrations = root.GetProperty("integrations");
    if (!ClosedObject(integrations, ["productShape", "arcShape"], ["productShape", "arcShape"], diagnostics, "manifest integrations")) return false;
    if (!ValidPin(integrations.GetProperty("productShape")) || !ValidPin(integrations.GetProperty("arcShape")))
    { diagnostics.Add("Manifest integration pins do not match the released pin format."); return false; }
    if (root.TryGetProperty("packages", out var packages))
    {
      if (!ClosedObject(packages, ["sdlc", "ui"], ["sdlc"], diagnostics, "manifest packages")) return false;
      foreach (var (key, name) in new[] { ("sdlc", "@juangcarmona/lighthouse-sdlc"), ("ui", "@juangcarmona/lighthouse-ui") })
        if (packages.TryGetProperty(key, out var package)
            && (!ClosedObject(package, ["name", "version"], ["name", "version"], diagnostics, "manifest " + key)
              || package.GetProperty("name").ValueKind != JsonValueKind.String || package.GetProperty("name").GetString() != name
              || !ValidPin(package.GetProperty("version"))))
        { diagnostics.Add("Manifest package " + key + " does not match the released package contract."); return false; }
    }
    if (root.TryGetProperty("repository", out var repository))
    {
      if (!ClosedObject(repository, ["layout", "publicSite", "conventions"], ["layout", "publicSite", "conventions"], diagnostics, "manifest repository")) return false;
      var layout = repository.GetProperty("layout");
      if (layout.ValueKind != JsonValueKind.Array || !layout.EnumerateArray().Select(value => value.ValueKind == JsonValueKind.String ? value.GetString() : null)
          .SequenceEqual(ManifestRepositoryLayout, StringComparer.Ordinal))
      { diagnostics.Add("Manifest repository layout does not match the released schema."); return false; }
      var publicSite = repository.GetProperty("publicSite");
      if (!ClosedObject(publicSite, ["enabled"], ["enabled"], diagnostics, "manifest public site")
          || publicSite.GetProperty("enabled").ValueKind is not (JsonValueKind.True or JsonValueKind.False))
      { diagnostics.Add("Manifest public site fields do not match the released schema."); return false; }
      var conventions = repository.GetProperty("conventions");
      if (!ClosedObject(conventions, ["commits", "branchPattern"], ["commits", "branchPattern"], diagnostics, "manifest conventions")
          || conventions.GetProperty("commits").ValueKind != JsonValueKind.String || conventions.GetProperty("commits").GetString() != "conventional"
          || conventions.GetProperty("branchPattern").ValueKind != JsonValueKind.String || string.IsNullOrWhiteSpace(conventions.GetProperty("branchPattern").GetString()))
      { diagnostics.Add("Manifest conventions do not match the released schema."); return false; }
    }
    var managed = root.GetProperty("managed");
    if (managed.ValueKind != JsonValueKind.Array || managed.GetArrayLength() == 0
        || managed.EnumerateArray().Any(value => value.ValueKind != JsonValueKind.String || string.IsNullOrWhiteSpace(value.GetString()))
        || managed.EnumerateArray().Select(value => value.GetString()).Distinct(StringComparer.Ordinal).Count() != managed.GetArrayLength())
    { diagnostics.Add("Manifest managed paths are empty, malformed or duplicated."); return false; }
    return true;
  }

  private static bool ValidUpgrade(JsonElement root, List<string> diagnostics)
  {
    var allowed = new HashSet<string>(["$schema", "schemaVersion", "package", "baseline", "capabilities", "adoption", "projections", "lastAppliedMigration", "files"], StringComparer.Ordinal);
    if (!ClosedObject(root, allowed, ["schemaVersion", "package", "baseline", "capabilities", "projections", "lastAppliedMigration", "files"], diagnostics, "upgrade")) return false;
    if (root.TryGetProperty("$schema", out var schemaUri) && schemaUri.ValueKind != JsonValueKind.String)
    { diagnostics.Add("The upgrade schema reference is not a string."); return false; }
    if (root.GetProperty("schemaVersion").ValueKind != JsonValueKind.Number || !root.GetProperty("schemaVersion").TryGetInt32(out var schema) || schema != 1
        || root.GetProperty("baseline").ValueKind != JsonValueKind.String || string.IsNullOrWhiteSpace(root.GetProperty("baseline").GetString())
        || root.GetProperty("lastAppliedMigration").ValueKind != JsonValueKind.String || string.IsNullOrWhiteSpace(root.GetProperty("lastAppliedMigration").GetString()))
    { diagnostics.Add("The Lighthouse upgrade document does not match schema version 1."); return false; }
    var package = root.GetProperty("package");
    if (!ClosedObject(package, ["name", "version"], ["name", "version"], diagnostics, "upgrade package")
        || package.GetProperty("name").ValueKind != JsonValueKind.String || package.GetProperty("name").GetString() != "@juangcarmona/lighthouse"
        || !ValidPin(package.GetProperty("version")))
    { diagnostics.Add("The Lighthouse upgrade package identity or pin is unsupported."); return false; }
    foreach (var section in new[] { "capabilities", "projections" })
      if (root.GetProperty(section).ValueKind != JsonValueKind.Object
          || root.GetProperty(section).EnumerateObject().Any(property => property.Value.ValueKind is not (JsonValueKind.String or JsonValueKind.Null)))
      { diagnostics.Add("The Lighthouse upgrade " + section + " object contains unsupported values."); return false; }
    if (root.TryGetProperty("adoption", out var adoption))
    {
      if (adoption.ValueKind != JsonValueKind.Object) { diagnostics.Add("The Lighthouse upgrade adoption section is not an object."); return false; }
      foreach (var entry in adoption.EnumerateObject())
      {
        var item = entry.Value;
        if (!ClosedObject(item, ["id", "status", "selection", "provider", "version", "catalogVersion", "nextAction", "recordPath"],
            ["id", "status", "provider", "version", "catalogVersion", "nextAction"], diagnostics, "adoption " + entry.Name)) return false;
        if (item.EnumerateObject().Any(property => property.Value.ValueKind is not (JsonValueKind.String or JsonValueKind.Null))
            || item.GetProperty("status").GetString() is not ("available" or "pending-adoption" or "adopted" or "declined" or "drifted" or "blocked" or "failed" or "unavailable")
            || item.TryGetProperty("selection", out var selection) && selection.GetString() is not ("selected-by-default" or "opt-in"))
        { diagnostics.Add("The adoption record contains an unsupported status, selection or value."); return false; }
      }
    }
    var files = root.GetProperty("files");
    if (files.ValueKind != JsonValueKind.Object) { diagnostics.Add("The upgrade files section is not an object."); return false; }
    foreach (var entry in files.EnumerateObject())
      if (!ClosedObject(entry.Value, ["hash", "scope"], ["hash", "scope"], diagnostics, "upgrade file " + entry.Name)
          || entry.Value.GetProperty("hash").ValueKind != JsonValueKind.String
          || !Regex.IsMatch(entry.Value.GetProperty("hash").GetString()!, @"^[a-f0-9]{64}$", RegexOptions.CultureInvariant)
          || entry.Value.GetProperty("scope").ValueKind != JsonValueKind.String
          || entry.Value.GetProperty("scope").GetString() is not ("harness" or "sdlc"))
      { diagnostics.Add("The upgrade file inventory contains an unsupported record."); return false; }
    return true;
  }

  private static bool ClosedObject(JsonElement value, IEnumerable<string> allowed, IEnumerable<string> required,
      List<string> diagnostics, string label)
  {
    var allowedSet = allowed.ToHashSet(StringComparer.Ordinal);
    var requiredSet = required.ToHashSet(StringComparer.Ordinal);
    if (value.ValueKind != JsonValueKind.Object || value.EnumerateObject().Any(property => !allowedSet.Contains(property.Name))
        || requiredSet.Any(name => !value.TryGetProperty(name, out _)))
    { diagnostics.Add("The " + label + " object has unsupported fields or is missing required fields."); return false; }
    return true;
  }

  private static bool ValidPin(JsonElement value) => value.ValueKind == JsonValueKind.Null
      || value.ValueKind == JsonValueKind.String && Regex.IsMatch(value.GetString()!, @"^(?:[0-9]+\.[0-9]+\.[0-9]+|[a-f0-9]{40})$", RegexOptions.CultureInvariant);

  private static void ProjectRawContract(string path, JsonElement root, List<RepositoryConfigurationFact> facts, List<string> diagnostics)
  {
    var identity = path == ".lighthouse/sdlc.json" ? "lighthouse-sdlc-config" : "lighthouse-commands-config";
    foreach (var property in new[] { "schemaVersion", "contractVersion" })
      AddScalar(root, property, identity, property == "schemaVersion" ? "schema-version" : "contract-version", facts, diagnostics, identity);
    if (root.TryGetProperty("package", out var package)) AddRawPackage(package, "package", identity, facts, diagnostics);
    if (root.TryGetProperty("packages", out var packages) && packages.ValueKind == JsonValueKind.Array)
      foreach (var (entry, index) in packages.EnumerateArray().Select((entry, index) => (entry, index)))
        AddRawPackage(entry, "packages/" + index.ToString(System.Globalization.CultureInfo.InvariantCulture), identity, facts, diagnostics);
    diagnostics.Add("No released interpretation schema is bundled for this contract; raw declarations are not validated.");
  }

  private static void AddRawPackage(JsonElement package, string pointer, string contract,
      List<RepositoryConfigurationFact> facts, List<string> diagnostics)
  {
    if (package.ValueKind != JsonValueKind.Object) { diagnostics.Add(pointer + " is not an object package declaration."); return; }
    var name = SourceJson.Text(package, "name");
    if (name is not { Length: > 0 and <= 200 }) { diagnostics.Add(pointer + "/name is missing or malformed."); return; }
    if (package.TryGetProperty("version", out var version) && version.ValueKind == JsonValueKind.String)
      facts.Add(new(pointer + "/version", name, "package-declaration", version.GetString()!, "", "", default, contract, "raw", ""));
    if (package.TryGetProperty("contractVersion", out var contractVersion)
        && contractVersion.ValueKind is JsonValueKind.Number or JsonValueKind.String)
      facts.Add(new(pointer + "/contractVersion", name, "contract-version", contractVersion.ToString(), "", "", default, contract, "raw", ""));
    facts.Add(new(pointer + "/name", name, "package-identity", name, "", "", default, contract, "raw", ""));
  }

  private static void AddPin(string pointer, string component, JsonElement value, string contract, List<RepositoryConfigurationFact> facts, List<string> diagnostics)
  {
    if (value.ValueKind == JsonValueKind.Object && value.TryGetProperty("version", out var version)) value = version;
    if (!ValidPin(value)) { diagnostics.Add(pointer + " is not a supported string or null pin."); return; }
    facts.Add(new(pointer, component, "manifest-pin", value.ValueKind == JsonValueKind.Null ? "null" : value.GetString()!, "", "", default, contract, "raw", ""));
  }

  private static void AddScalar(JsonElement root, string property, string component, string kind,
      List<RepositoryConfigurationFact> facts, List<string> diagnostics, string? contract = null)
  {
    if (!root.TryGetProperty(property, out var value)) return;
    if (value.ValueKind is not (JsonValueKind.String or JsonValueKind.Number)) { diagnostics.Add(property + " has an unsupported value type."); return; }
    facts.Add(new(property, component, kind, value.ToString(), "", "", default, contract ?? component, "raw", ""));
  }
}
