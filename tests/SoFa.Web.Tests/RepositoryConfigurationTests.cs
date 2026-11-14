using System.Text;
using System.Net;
using System.Text.Json;
using JuanGCarmona.SoFa.Modules.Workspace;
using JuanGCarmona.SoFa.Modules.Workspace.Identity;
using JuanGCarmona.SoFa.Modules.Workspace.Source;
using Microsoft.AspNetCore.Authentication;

namespace SoFa.Web.Tests;

public sealed class RepositoryConfigurationTests
{
  private static readonly string[] ExpectedResolutions = ["0.12.68", "0.12.66"];
  private const string Manifest = """{"schemaVersion":1,"baseline":"0.1.0","product":"Example","profiles":["node-tooling"],"integrations":{"productShape":"0.23.0","arcShape":null},"packages":{"sdlc":{"name":"@juangcarmona/lighthouse-sdlc","version":"0.12.0"}},"managed":[".github/workflows/ci.yml"]}""";
  private const string Upgrade = """{"schemaVersion":1,"package":{"name":"@juangcarmona/lighthouse","version":"0.12.68"},"baseline":"0.1.0","capabilities":{"harness":"1","sdlc":null},"adoption":{"labels":{"id":"labels","status":"drifted","selection":"opt-in","provider":"github","version":"4","catalogVersion":"0.5.0","nextAction":"#77","recordPath":".lighthouse/reconciliation/labels.json"}},"projections":{"sdlc":"1"},"lastAppliedMigration":"upgrade-0.12.68","files":{"LIGHTHOUSE.md":{"hash":"aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa","scope":"harness"}}}""";
  private static readonly JsonSerializerOptions JsonOptions = new()
  { DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull };
  [Theory]
  [InlineData("contents/lighthouse.json?ref=aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa")]
  [InlineData("contents/package.json?ref=aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa")]
  [InlineData("contents/package-lock.json?ref=aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa")]
  [InlineData("contents/.lighthouse/upgrade.json?ref=aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa")]
  [InlineData("contents/.github/workflows/ci.yml?ref=aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa")]
  public void OnlyAllowlistedConfigurationSourcesCanUseTheSharedReader(string path) => Assert.True(ControlReadPaths.Allowed(path));

  [Theory]
  [InlineData("contents/.env?ref=aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa")]
  [InlineData("contents/other.json?ref=aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa")]
  [InlineData("contents/../secrets.json?ref=aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa")]
  [InlineData("contents/package.json?ref=main")]
  public void ArbitraryPathsAndMutableRefsRemainUnavailable(string path) => Assert.False(ControlReadPaths.Allowed(path));

  [Fact]
  public void NpmDeclarationsAndLockResolutionsRemainSeparateAndKeepNestedLocations()
  {
    const string manifest = """{"dependencies":{"@juangcarmona/lighthouse":"^0.12.65"},"devDependencies":{"@juangcarmona/productshape":"0.23.0"}}""";
    const string lockfile = """{"lockfileVersion":3,"packages":{"":{"name":"sample"},"node_modules/@juangcarmona/lighthouse":{"version":"0.12.68"},"apps/web/node_modules/@juangcarmona/lighthouse":{"version":"0.12.66"}}}""";
    Assert.True(RepositoryConfigurationProjector.TryParse("package.json", Encoding.UTF8.GetBytes(manifest), out var declared, out _));
    Assert.True(RepositoryConfigurationProjector.TryParse("package-lock.json", Encoding.UTF8.GetBytes(lockfile), out var resolved, out _));
    var declaration = Assert.Single(declared, fact => fact.Component == "@juangcarmona/lighthouse");
    Assert.Equal("^0.12.65", declaration.RawValue);
    Assert.Equal("declared-package", declaration.FactKind);
    Assert.Equal(ExpectedResolutions, resolved.Where(fact => fact.Component == "@juangcarmona/lighthouse").Select(fact => fact.RawValue));
    Assert.All(resolved, fact => Assert.Equal("lock-resolved-package", fact.FactKind));
  }

  [Fact]
  public void DuplicateKeysAndUnsupportedLockVersionsNeverBecomeValidatedFacts()
  {
    Assert.False(RepositoryConfigurationProjector.TryParse("package.json",
        Encoding.UTF8.GetBytes("{\"dependencies\":{},\"dependencies\":{}}"), out var duplicate, out var duplicateDiagnostics));
    Assert.Empty(duplicate);
    Assert.Contains(duplicateDiagnostics, reason => reason.Contains("duplicate", StringComparison.OrdinalIgnoreCase));
    Assert.True(RepositoryConfigurationProjector.TryParse("package-lock.json",
        Encoding.UTF8.GetBytes("{\"lockfileVersion\":2,\"packages\":{}}"), out var unsupported, out var diagnostics));
    Assert.Empty(unsupported);
    Assert.Contains(diagnostics, reason => reason.Contains("version 3", StringComparison.Ordinal));
  }

  [Fact]
  public void NpmApplicabilityIsLimitedToTheSupportedRootPackageDeclaration()
  {
    Assert.True(RepositoryConfigurationProjector.TryParse("package.json", Encoding.UTF8.GetBytes("{\"name\":\"sample\"}"), out var facts, out _));
    var applicability = Assert.Single(facts);
    Assert.Equal("applicability", applicability.FactKind);
    Assert.Equal("supported-root-scope-only", applicability.Interpretation);
  }

  [Fact]
  public void ReleasedManifestAndUpgradeFactsKeepTheirSeparateDimensions()
  {
    Assert.True(RepositoryConfigurationProjector.TryParse("lighthouse.json", Encoding.UTF8.GetBytes(Manifest), out var manifest, out var manifestDiagnostics));
    Assert.Empty(manifestDiagnostics);
    Assert.Contains(manifest, fact => fact.FactKind == "manifest-baseline" && fact.RawValue == "0.1.0");
    Assert.Contains(manifest, fact => fact.FactKind == "profile" && fact.RawValue == "node-tooling");
    Assert.Contains(manifest, fact => fact.Component == "@juangcarmona/productshape" && fact.RawValue == "0.23.0");
    Assert.Contains(manifest, fact => fact.Component == "@juangcarmona/lighthouse-sdlc" && fact.RawValue == "0.12.0");
    Assert.Contains(manifest, fact => fact.FactKind == "managed-path" && fact.RawValue == ".github/workflows/ci.yml");

    Assert.True(RepositoryConfigurationProjector.TryParse(".lighthouse/upgrade.json", Encoding.UTF8.GetBytes(Upgrade), out var upgrade, out var upgradeDiagnostics));
    Assert.Empty(upgradeDiagnostics);
    Assert.Contains(upgrade, fact => fact.FactKind == "declared-package" && fact.RawValue == "0.12.68");
    Assert.Contains(upgrade, fact => fact.FactKind == "capability-version" && fact.RawValue == "1");
    var action = Assert.Single(upgrade, fact => fact.FactKind == "adoption-nextAction");
    Assert.Equal("#77", action.RawValue);
    Assert.Equal("#77", action.RelatedWorkUrl);
  }

  [Theory]
  [InlineData("{\"schemaVersion\":2,\"baseline\":\"0.1.0\",\"product\":\"Example\",\"profiles\":[],\"integrations\":{\"productShape\":null,\"arcShape\":null},\"managed\":[\"ci.yml\"]}")]
  [InlineData("{\"schemaVersion\":1,\"baseline\":\"0.1.0\",\"product\":\"Example\",\"profiles\":[],\"integrations\":{\"productShape\":null,\"arcShape\":null},\"managed\":[\"ci.yml\"],\"unknown\":true}")]
  public void UnsupportedReleasedManifestCombinationsDoNotProduceInterpretedFacts(string json)
  {
    Assert.False(RepositoryConfigurationProjector.TryParse("lighthouse.json", Encoding.UTF8.GetBytes(json), out var facts, out var diagnostics));
    Assert.Empty(facts);
    Assert.NotEmpty(diagnostics);
  }

  [Fact]
  public void RawControlPlanePackageAndAliasContractVersionsStayDistinct()
  {
    const string commands = """{"schemaVersion":1,"contractVersion":1,"packages":[{"name":"@juangcarmona/lighthouse","version":"0.12.68","contractVersion":1}]}""";
    Assert.True(RepositoryConfigurationProjector.TryParse(".lighthouse/commands.json", Encoding.UTF8.GetBytes(commands), out var facts, out var diagnostics));
    Assert.Contains(facts, fact => fact.FactKind == "package-declaration" && fact.RawValue == "0.12.68");
    Assert.Contains(facts, fact => fact.FactKind == "contract-version" && fact.RawValue == "1");
    Assert.Contains(diagnostics, reason => reason.Contains("No released interpretation schema", StringComparison.Ordinal));
  }

  [Fact]
  public void ReleasedSdlcContractFixtureIsUnsupportedByItsClosedDeclaredSchema()
  {
    var root = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../../../"));
    var evidence = Path.Combine(root, "openspec/changes/issue-53-proposal/evidence/schemas/sdlc-0.22.4");
    using var contract = JsonDocument.Parse(File.ReadAllBytes(Path.Combine(evidence, "contract.json")));
    using var schema = JsonDocument.Parse(File.ReadAllBytes(Path.Combine(evidence, "sdlc-contract.schema.json")));
    var declaredAssets = contract.RootElement.GetProperty("assets");
    var schemaAssets = schema.RootElement.GetProperty("properties").GetProperty("assets");
    var allowedAssets = schemaAssets.GetProperty("properties").EnumerateObject().Select(property => property.Name).ToHashSet(StringComparer.Ordinal);
    var requiredAssets = schemaAssets.GetProperty("required").EnumerateArray().Select(value => value.GetString()!).ToHashSet(StringComparer.Ordinal);
    var declaredNames = declaredAssets.EnumerateObject().Select(property => property.Name).ToHashSet(StringComparer.Ordinal);

    Assert.Equal("sdlc-0.22.4", Path.GetFileName(evidence));
    Assert.True(schemaAssets.GetProperty("additionalProperties").ValueKind == JsonValueKind.False);
    Assert.Contains("browserE2eSchema", declaredNames);
    Assert.Contains("commandAliases", declaredNames);
    Assert.Contains("commandAliasesSchema", declaredNames);
    Assert.Contains("capabilityCatalog", declaredNames);
    Assert.Contains("capabilityCatalogSchema", declaredNames);
    Assert.Equal(new HashSet<string>(["browserE2eSchema", "commandAliases", "commandAliasesSchema", "capabilityCatalog", "capabilityCatalogSchema"], StringComparer.Ordinal),
        declaredNames.Except(allowedAssets).ToHashSet(StringComparer.Ordinal));
    Assert.All(requiredAssets, name => Assert.Contains(name, declaredNames));
  }

  [Fact]
  public async Task ConfigurationReadBindsEveryFileToOneImmutableCommitAndRequiresContentsGrant()
  {
    using var fixture = new Fixture(contentsGrant: "read");
    var session = await fixture.Session();
    var result = await fixture.Query.Handle(new(session, "example/repository"), CancellationToken.None);
    Assert.Equal("available", result.Availability);
    Assert.Equal("complete", result.Coverage);
    Assert.Equal("main", result.ConfiguredBranch);
    Assert.Equal("a".PadRight(40, 'a'), result.ObservedRevision);
    Assert.Equal(7, result.Sources.Count);
    Assert.All(result.Sources, source => Assert.Equal(result.ObservedRevision, source.Revision));
    Assert.Equal(7, fixture.Handler.Paths.Count(path => path.Contains("/contents/", StringComparison.Ordinal)));
    Assert.Contains("/repos/example/repository/commits/main", fixture.Handler.Paths);
    Assert.DoesNotContain(fixture.Handler.Paths, path => path.Contains("/issues/", StringComparison.Ordinal)
        || path.Contains("/dispatches", StringComparison.Ordinal));
    Assert.Contains(result.Facts, fact => fact.RelatedWorkUrl == "https://github.com/example/repository/issues/77");
    var priorReadCount = fixture.Handler.Paths.Count(path => path.Contains("/contents/", StringComparison.Ordinal));
    var reused = await fixture.Query.Handle(new(session, "example/repository"), CancellationToken.None);
    Assert.Equal(result.ObservedAt, reused.ObservedAt);
    Assert.Equal(priorReadCount, fixture.Handler.Paths.Count(path => path.Contains("/contents/", StringComparison.Ordinal)));
    fixture.Clock.Advance(TimeSpan.FromSeconds(16));
    fixture.Handler.FailContentReads = true;
    var stale = await fixture.Query.Handle(new(session, "example/repository"), CancellationToken.None);
    Assert.Equal("stale", stale.Freshness);
    Assert.Equal(result.ObservedAt, stale.ObservedAt);
    Assert.Contains(stale.Reasons, reason => reason.Contains("earlier commit", StringComparison.Ordinal));
    fixture.Handler.FailContentReads = false;
    var refreshed = await fixture.Query.Handle(new(session, "example/repository"), CancellationToken.None);
    Assert.Equal("fresh", refreshed.Freshness);
    Assert.Equal(priorReadCount * 2 + 1, fixture.Handler.Paths.Count(path => path.Contains("/contents/", StringComparison.Ordinal)));
    fixture.Handler.ContentsGrant = null;
    var lost = await fixture.Query.Handle(new(session, "example/repository"), CancellationToken.None);
    Assert.Equal("denied", lost.Availability);
    Assert.Equal("unknown", lost.Freshness);
    Assert.Empty(lost.Facts);

    using var deniedFixture = new Fixture(contentsGrant: null);
    var denied = await deniedFixture.Query.Handle(new(await deniedFixture.Session(), "example/repository"), CancellationToken.None);
    Assert.Equal("denied", denied.Availability);
    Assert.Equal("unknown", denied.Freshness);
    Assert.DoesNotContain(deniedFixture.Handler.Paths, path => path.Contains("/contents/", StringComparison.Ordinal)
        || path.Contains("/commits/", StringComparison.Ordinal));
  }

  [Fact]
  public async Task ContentsPermissionLossMidReadDiscardsAlreadyCollectedFacts()
  {
    using var fixture = new Fixture(contentsGrant: "read");
    fixture.Handler.DenyContentReadAt = 3;

    var result = await fixture.Query.Handle(new(await fixture.Session(), "example/repository"), CancellationToken.None);

    Assert.Equal("denied", result.Availability);
    Assert.Empty(result.Facts);
    Assert.Contains(result.Reasons, reason => reason.Contains("facts were cleared", StringComparison.Ordinal));
  }

  [Fact]
  public async Task ManagedWorkflowReadsAreLimitedToTwoAndUnfollowablePathsRemainPartial()
  {
    using var fixture = new Fixture(contentsGrant: "read");
    fixture.Handler.ContentOverrides["lighthouse.json"] = Manifest.Replace("\".github/workflows/ci.yml\"", "\".github/workflows/ci.yml\",\".github/workflows/two.yml\",\".github/workflows/three.yml\"", StringComparison.Ordinal);

    var bounded = await fixture.Query.Handle(new(await fixture.Session(), "example/repository"), CancellationToken.None);

    Assert.Equal(8, bounded.Sources.Count);
    Assert.Equal(8, JsonSerializer.SerializeToElement(bounded.Limits).GetProperty("reads").GetInt32());
    Assert.Equal(2, bounded.Facts.Count(fact => fact.FactKind == "source-evidence-sha256"));
    Assert.Equal("partial", bounded.Coverage);

    using var unsafeFixture = new Fixture(contentsGrant: "read");
    unsafeFixture.Handler.ContentOverrides["lighthouse.json"] = Manifest.Replace(".github/workflows/ci.yml", "../../secrets.yml", StringComparison.Ordinal);
    var unsafePath = await unsafeFixture.Query.Handle(new(await unsafeFixture.Session(), "example/repository"), CancellationToken.None);
    Assert.DoesNotContain(unsafeFixture.Handler.Paths, path => path.Contains("secrets", StringComparison.Ordinal));
    Assert.Equal("partial", unsafePath.Coverage);
    Assert.Contains(unsafePath.Reasons, reason => reason.Contains("managed workflow", StringComparison.OrdinalIgnoreCase));
  }

  [Fact]
  public async Task FileAndAggregateByteBudgetsReportPartialCoverageWithoutTruncation()
  {
    using var fileFixture = new Fixture(contentsGrant: "read");
    fileFixture.Handler.ContentOverrides["package.json"] = new string(' ', 256 * 1024 + 1);
    var fileBound = await fileFixture.Query.Handle(new(await fileFixture.Session(), "example/repository"), CancellationToken.None);
    Assert.Contains(fileBound.Sources, source => source.Path == "package.json" && source.Availability == "unavailable"
        && source.Diagnostics.Any(reason => reason.Contains("per-file limit", StringComparison.Ordinal)));
    Assert.Equal("partial", fileBound.Coverage);
    Assert.DoesNotContain(fileBound.Facts, fact => fact.SourcePath == "package.json");

    using var aggregateFixture = new Fixture(contentsGrant: "read");
    foreach (var path in new[] { "lighthouse.json", ".lighthouse/upgrade.json", ".lighthouse/sdlc.json", ".lighthouse/commands.json" })
      aggregateFixture.Handler.ContentOverrides[path] = new string(' ', 256 * 1024);
    aggregateFixture.Handler.ContentOverrides["package.json"] = "{}";
    var aggregate = await aggregateFixture.Query.Handle(new(await aggregateFixture.Session(), "example/repository"), CancellationToken.None);
    Assert.Equal(4, aggregateFixture.Handler.Paths.Count(path => path.Contains("/contents/", StringComparison.Ordinal)));
    Assert.Equal("partial", aggregate.Coverage);
    Assert.Contains(aggregate.Reasons, reason => reason.Contains("aggregate decoded content", StringComparison.OrdinalIgnoreCase));
    Assert.DoesNotContain(aggregateFixture.Handler.Paths, path => path.Contains("package-lock.json", StringComparison.Ordinal));
  }

  [Fact]
  public async Task RevisionAndRelatedWorkLinksAreBoundToSafeGitHubTargets()
  {
    using var fixture = new Fixture(contentsGrant: "read");
    fixture.Handler.ContentOverrides[".lighthouse/upgrade.json"] = Upgrade.Replace("#77", "https://evil.example/issues/77", StringComparison.Ordinal);
    var result = await fixture.Query.Handle(new(await fixture.Session(), "example/repository"), CancellationToken.None);
    Assert.All(result.Sources.Where(source => source.Availability == "available"), source =>
      Assert.Matches("^https://github\\.com/example/repository/blob/[a-f0-9]{40}/", source.SourceUrl));
    var nextAction = Assert.Single(result.Facts, fact => fact.FactKind == "adoption-nextAction");
    Assert.Equal("https://evil.example/issues/77", nextAction.RawValue);
    Assert.Null(nextAction.RelatedWorkUrl);
  }

  [Fact]
  public async Task ConfigurationProjectionCacheDoesNotCrossOwnerSessions()
  {
    using var fixture = new Fixture(contentsGrant: "read");
    var first = await fixture.Query.Handle(new(await fixture.Session(), "example/repository"), CancellationToken.None);
    var firstReads = fixture.Handler.Paths.Count(path => path.Contains("/contents/", StringComparison.Ordinal));
    var second = await fixture.Query.Handle(new(await fixture.Session(), "example/repository"), CancellationToken.None);
    Assert.Equal("available", first.Availability);
    Assert.Equal("available", second.Availability);
    Assert.Equal(firstReads * 2, fixture.Handler.Paths.Count(path => path.Contains("/contents/", StringComparison.Ordinal)));
  }

  [Fact]
  public async Task AConnectionReplacementWhileReadingCannotPublishTheLateProjection()
  {
    using var fixture = new Fixture(contentsGrant: "read");
    var session = await fixture.Session();
    var oldConnection = session.GitHub.Current!;
    fixture.Handler.PauseFirstContentRead = true;
    var pending = fixture.Query.Handle(new(session, "example/repository"), CancellationToken.None);
    await fixture.Handler.ContentReadStarted.Task.WaitAsync(TimeSpan.FromSeconds(5));
    lock (session.Sync)
    {
      session.GitHub.Generation++;
      session.GitHub.Current = new(72, "replacement-user", new("replacement-access", "replacement-refresh",
          fixture.Clock.GetUtcNow().AddHours(1), fixture.Clock.GetUtcNow().AddHours(4)), fixture.Clock.GetUtcNow());
    }
    oldConnection.Stopped.Cancel();
    fixture.Handler.ReleaseContentRead.TrySetResult();

    var result = await pending;

    Assert.Equal("disconnected", result.Availability);
    Assert.Empty(result.Facts);
    Assert.DoesNotContain(result.Sources, source => source.Availability == "available");
  }

  private sealed class Fixture : IDisposable
  {
    private readonly TestClock clock = new();
    private readonly OwnerSessions sessions;
    private readonly GitHubTransport transport;
    internal SourceHandler Handler { get; } = new();
    internal TestClock Clock => clock;
    internal GetRepositoryConfigurationHandler Query { get; }

    internal Fixture(string? contentsGrant)
    {
      Handler.ContentsGrant = contentsGrant;
      transport = new(Handler);
      sessions = new(OwnerAdmissionTests.Profile(), clock);
      var settings = new WorkspaceOptions
      {
        Origin = new Uri("https://workspace.invalid"),
        IdentityMode = "Development",
        Identity = OwnerAdmissionTests.Profile(),
        Repositories = ["example/repository"],
        RepositoryBranches = new Dictionary<string, string> { ["example/repository"] = "main" },
        GitHubClientId = "owner-app",
        GitHubAppId = 9001
      };
      var authorization = new GitHubAuthorization(settings, sessions, transport, clock);
      var access = new RepositoryAccess(settings, sessions, authorization, transport, clock);
      Query = new(settings, sessions, authorization, access, transport, clock);
    }

    internal async Task<OwnerSession> Session()
    {
      var principal = OwnerAdmissionTests.Principal(authenticationTime: clock.GetUtcNow().ToUnixTimeSeconds().ToString(System.Globalization.CultureInfo.InvariantCulture));
      var session = sessions.Get(await sessions.StoreAsync(new AuthenticationTicket(principal, "owner")))!;
      session.GitHub.Current = new(71, "verified-user", new("isolated-source-access", "isolated-source-refresh",
          clock.GetUtcNow().AddHours(1), clock.GetUtcNow().AddHours(4)), clock.GetUtcNow());
      return session;
    }

    public void Dispose() { sessions.Dispose(); transport.Dispose(); }
  }

  private sealed class SourceHandler : HttpMessageHandler
  {
    internal string? ContentsGrant { get; set; }
    internal bool FailContentReads { get; set; }
    internal int? DenyContentReadAt { get; set; }
    internal Dictionary<string, string> ContentOverrides { get; } = new(StringComparer.Ordinal);
    internal bool PauseFirstContentRead { get; set; }
    internal TaskCompletionSource ContentReadStarted { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
    internal TaskCompletionSource ReleaseContentRead { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
    private int contentReadCount;
    internal List<string> Paths { get; } = [];
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
      Assert.Equal(HttpMethod.Get, request.Method);
      Assert.Equal("api.github.com", request.RequestUri!.Host);
      Assert.Equal("isolated-source-access", request.Headers.Authorization?.Parameter);
      var path = request.RequestUri.AbsolutePath; Paths.Add(path);
      if (path.Contains("/contents/", StringComparison.Ordinal)) contentReadCount++;
      if (PauseFirstContentRead && contentReadCount == 1)
      {
        ContentReadStarted.TrySetResult();
        await ReleaseContentRead.Task.ConfigureAwait(false);
      }
      if (DenyContentReadAt == contentReadCount)
        return new HttpResponseMessage(HttpStatusCode.Forbidden)
        { Content = new StringContent("{}", Encoding.UTF8, "application/json") };
      if (FailContentReads && path.Contains("/contents/", StringComparison.Ordinal))
        return new HttpResponseMessage(HttpStatusCode.ServiceUnavailable)
        { Content = new StringContent("{}", Encoding.UTF8, "application/json") };
      object value = path switch
      {
        "/repos/example/repository" => new
        {
          id = 22,
          full_name = "example/repository",
          visibility = "private",
          @private = true,
          permissions = new { pull = true, push = false, maintain = false, admin = false }
        },
        "/user/installations" => new
        {
          total_count = 1,
          installations = new[] { new { id = 9, app_id = 9001,
          account = new { login = "example" }, permissions = new { metadata = "read", issues = "read", actions = "read", variables = "read", contents = ContentsGrant, pull_requests = "read" } } }
        },
        "/user/installations/9/repositories" => new { total_count = 1, repositories = new[] { new { id = 22, full_name = "example/repository", visibility = "private", @private = true } } },
        "/repos/example/repository/commits/main" => new { sha = new string('a', 40) },
        _ when path.StartsWith("/repos/example/repository/contents/", StringComparison.Ordinal) => Content(path),
        _ => throw new InvalidOperationException("Unexpected configuration source read: " + path)
      };
      return new HttpResponseMessage(HttpStatusCode.OK)
      { Content = new StringContent(JsonSerializer.Serialize(value, JsonOptions), Encoding.UTF8, "application/json") };
    }

    private object Content(string path)
    {
      var file = path["/repos/example/repository/contents/".Length..];
      var text = ContentOverrides.GetValueOrDefault(file) ?? file switch
      {
        "lighthouse.json" => Manifest,
        ".lighthouse/upgrade.json" => Upgrade,
        "package.json" => "{\"dependencies\":{\"@juangcarmona/lighthouse\":\"0.12.68\"}}",
        "package-lock.json" => "{\"lockfileVersion\":3,\"packages\":{}}",
        ".github/workflows/ci.yml" => "name: CI\nsteps: []\n",
        _ => "{}"
      };
      return new { type = "file", encoding = "base64", content = Convert.ToBase64String(Encoding.UTF8.GetBytes(text)) };
    }
  }
}
