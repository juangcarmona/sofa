using System.Net;
using System.Text.Json;
using SoFa.Attention;

namespace SoFa.GitHub;

public sealed class GitHubEvidenceSource(HttpClient client, string owner) : IEvidenceSource
{
  private const int MaxPages = 10;
  public async Task<RepositoryEvidence> CollectAsync(RepositoryMapping repository, CancellationToken cancellationToken)
  {
    var prefix = "repos/" + repository.Name + "/";
    // Separate category failures so absent labels or an API denial cannot hide other evidence.
    var categories = new List<CategoryEvidence>();
    foreach (var category in Enum.GetValues<Category>())
    {
      try
      {
        categories.Add(category switch
        {
          Category.Approval => await Approvals(repository, prefix, cancellationToken),
          Category.Checks => await Checks(prefix, cancellationToken),
          Category.Blockers => await Blockers(repository, prefix, cancellationToken),
          _ => await Releases(repository, prefix, cancellationToken)
        });
      }
      catch (CollectionException ex)
      {
        categories.Add(new(category, ex.Partial ? Coverage.Incomplete : Coverage.Unavailable, ex.SafeMessage, [], ex.Reason));
      }
      catch (Exception ex)
      {
        categories.Add(new(category, Coverage.Unavailable, "Source evidence could not be read.", [], ReasonFor(ex)));
      }
    }
    return new(repository.Name, categories.AsReadOnly());
  }

  private async Task<CategoryEvidence> Approvals(RepositoryMapping mapping, string prefix, CancellationToken token)
  {
    var prs = await List(prefix + "pulls?state=open", null, token);
    var observations = prs.Items.Select(pr => new Observation(
        Text(pr, "number"), Text(pr, "title"), Text(pr, "html_url"), Time(pr, "updated_at"), "open",
        Author: Text(pr.GetProperty("user"), "login"),
        ReviewRequested: pr.GetProperty("requested_reviewers").EnumerateArray().Any(r => Text(r, "login").Equals(owner, StringComparison.OrdinalIgnoreCase)),
        OwnerReview: HasLabel(pr, mapping.OwnerReviewLabel), IsDraft: Bool(pr, "draft"), IsOpen: Text(pr, "state") == "open"));
    return new(Category.Approval, prs.Complete ? Coverage.Complete : Coverage.Incomplete,
        mapping.OwnerReviewLabel is null ? "Authored PRs and direct review requests inspected; owner-review label convention unconfigured." : "Authored PRs, direct review requests and owner-review label inspected.",
        Array.AsReadOnly(observations.ToArray()), prs.Reason);
  }

  private async Task<CategoryEvidence> Blockers(RepositoryMapping mapping, string prefix, CancellationToken token)
  {
    if (mapping.BlockerLabel is null) return new(Category.Blockers, Coverage.Unconfigured, "No explicit blocker convention mapped.", []);
    var issues = await List(prefix + "issues?state=open&labels=" + Uri.EscapeDataString(mapping.BlockerLabel), null, token);
    return new(Category.Blockers, issues.Complete ? Coverage.Complete : Coverage.Incomplete, "Explicit label: " + mapping.BlockerLabel,
        Array.AsReadOnly(issues.Items.Where(i => !i.TryGetProperty("pull_request", out _)).Select(i =>
            new Observation(Text(i, "number"), Text(i, "title"), Text(i, "html_url"), Time(i, "updated_at"), "open",
                IsOpen: Text(i, "state") == "open", ExplicitBlock: HasLabel(i, mapping.BlockerLabel))).ToArray()), issues.Reason);
  }

  private async Task<CategoryEvidence> Checks(string prefix, CancellationToken token)
  {
    var prs = await List(prefix + "pulls?state=open", null, token);
    var observations = new List<Observation>();
    bool incomplete = !prs.Complete;
    var reason = prs.Reason;
    foreach (var summary in prs.Items)
    {
      var number = Text(summary, "number");
      try
      {
        var pr = await Get(prefix + "pulls/" + number, token);
        var head = Text(pr.GetProperty("head"), "sha");
        var merge = Text(pr, "merge_commit_sha");
        var candidate = new List<Observation>();
        foreach (var revision in new[] { head, merge }.Where(x => x.Length > 0).Distinct())
        {
          var checks = await List(prefix + "commits/" + revision + "/check-runs?filter=latest", "check_runs", token);
          if (!checks.Complete) throw new CollectionException("Incomplete check page.", true, checks.Reason);
          foreach (var check in checks.Items)
          {
            var app = check.TryGetProperty("app", out var a) ? Text(a, "id") : "unknown";
            var state = Text(check, "status") == "completed" ? Text(check, "conclusion") : Text(check, "status");
            candidate.Add(new(number + ":check:" + revision + ":" + app + ":" + Text(check, "name"),
                "PR #" + number + " · " + Text(check, "name"), Text(check, "html_url").Length > 0 ? Text(check, "html_url") : Text(pr, "html_url") + "/checks",
                Time(check, "completed_at", "started_at"), state, Text(check, "head_sha"), revision,
                Sequence: Number(check, "id")));
          }
          var statuses = await List(prefix + "commits/" + revision + "/statuses?", null, token);
          if (!statuses.Complete) throw new CollectionException("Incomplete status page.", true, statuses.Reason);
          candidate.AddRange(statuses.Items.Select(status => new Observation(
              number + ":status:" + revision + ":" + Text(status, "context"),
              "PR #" + number + " · " + Text(status, "context"), Text(pr, "html_url") + "/checks",
              Time(status, "updated_at", "created_at"), Text(status, "state"), revision, revision,
              Sequence: Number(status, "id"))));
        }
        var after = await Get(prefix + "pulls/" + number, token);
        if (Text(after.GetProperty("head"), "sha") != head || Text(after, "merge_commit_sha") != merge || Text(after, "state") != "open")
        {
          incomplete = true;
          reason = FailureReason.RevisionChanged; // Never publish observations from a superseded revision.
          continue;
        }
        observations.AddRange(candidate);
      }
      catch (Exception ex)
      {
        incomplete = true;
        reason = ReasonFor(ex);
        if (token.IsCancellationRequested) break;
      }
    }
    return new(Category.Checks, incomplete ? Coverage.Incomplete : Coverage.Complete,
        incomplete ? "Some PR checks could not be collected or their revision changed. Refresh to retry." : "Current PR head and merge revision checks inspected.",
        observations.AsReadOnly(), reason);
  }

  private async Task<CategoryEvidence> Releases(RepositoryMapping mapping, string prefix, CancellationToken token)
  {
    if (mapping.ReleaseWorkflow is null) return new(Category.Releases, Coverage.Unconfigured, "No publishing workflow mapped.", []);
    // GitHub orders workflow runs newest first; inspect the newest target, never historical failures.
    var result = await Get(prefix + "actions/workflows/" + Uri.EscapeDataString(mapping.ReleaseWorkflow) + "/runs?per_page=1", token);
    var runs = result.GetProperty("workflow_runs");
    if (runs.GetArrayLength() == 0) return new(Category.Releases, Coverage.Complete, "Mapped workflow has no runs.", []);
    var run = runs[0];
    if (mapping.ReleaseTarget != "tag")
      return new(Category.Releases, Coverage.Incomplete, "Workflow mapped, but actual publication target is not reliably exposed in run metadata.", [], FailureReason.TargetUnknown);
    var tag = Text(run, "head_branch");
    var sha = Text(run, "head_sha");
    if (Text(run, "event") is not ("release" or "push") || tag.Length == 0)
      return new(Category.Releases, Coverage.Incomplete, "Newest run does not identify a supported release target.", [], FailureReason.TargetUnknown);
    var reference = await Get(prefix + "git/ref/tags/" + Uri.EscapeDataString(tag), token);
    var obj = reference.GetProperty("object");
    for (int depth = 0; Text(obj, "type") == "tag" && depth < 3; depth++)
      obj = (await Get(prefix + "git/tags/" + Text(obj, "sha"), token)).GetProperty("object");
    if (Text(obj, "type") != "commit" || Text(obj, "sha") != sha)
      return new(Category.Releases, Coverage.Incomplete, "Release tag cannot be verified against the run revision.", [], FailureReason.TargetUnknown);
    // Fetch the run itself so a later retry is authoritative over the listing.
    var latest = await Get(prefix + "actions/runs/" + Text(run, "id"), token);
    if (Text(latest, "head_sha") != sha || Text(latest, "head_branch") != tag)
      return new(Category.Releases, Coverage.Incomplete, "Release target changed while collecting.", [], FailureReason.TargetUnknown);
    var state = Text(latest, "status") == "completed" ? Text(latest, "conclusion") : Text(latest, "status");
    return new(Category.Releases, Coverage.Complete, "Newest verified target: " + tag,
        [new Observation(Text(latest, "id"), mapping.ReleaseWorkflow + " · " + tag,
                Text(latest, "html_url"), Time(latest, "updated_at"), state, sha, sha,
                Sequence: Number(latest, "run_attempt"), Target: tag)]);
  }

  private sealed record PageResult(List<JsonElement> Items, bool Complete, FailureReason Reason = FailureReason.None);

  private async Task<PageResult> List(string path, string? property, CancellationToken token)
  {
    var items = new List<JsonElement>();
    for (int page = 1; page <= MaxPages; page++)
    {
      JsonElement result;
      try { result = await Get(path + (path.Contains('?') ? "&" : "?") + "per_page=100&page=" + page, token); }
      catch (Exception ex) when (page > 1) { return new(items, false, ReasonFor(ex)); }
      var values = property is null ? result : result.GetProperty(property);
      items.AddRange(values.EnumerateArray());
      if (values.GetArrayLength() < 100) return new(items, true);
    }
    return new(items, false, FailureReason.PaginationLimit);
  }

  private async Task<JsonElement> Get(string path, CancellationToken token)
  {
    for (int attempt = 0; attempt < 2; attempt++)
    {
      using var timeout = CancellationTokenSource.CreateLinkedTokenSource(token);
      timeout.CancelAfter(TimeSpan.FromSeconds(10));
      using var response = await client.GetAsync(path, timeout.Token);
      if ((int)response.StatusCode >= 500 && attempt == 0)
      {
        await Task.Delay(200, token);
        continue;
      }
      if (!response.IsSuccessStatusCode)
        throw new CollectionException(response.StatusCode is HttpStatusCode.Forbidden or HttpStatusCode.TooManyRequests
            ? "Access denied or rate limited." : "Source request unavailable (HTTP " + (int)response.StatusCode + ").",
          reason: response.StatusCode is HttpStatusCode.Forbidden or HttpStatusCode.TooManyRequests ? FailureReason.AccessDeniedOrRateLimited : FailureReason.SourceUnavailable);
      using var document = JsonDocument.Parse(await response.Content.ReadAsStreamAsync(timeout.Token));
      return document.RootElement.Clone();
    }
    throw new CollectionException("Source unavailable after bounded retry.");
  }

  private static string Text(JsonElement value, string name) => value.TryGetProperty(name, out var property) && property.ValueKind != JsonValueKind.Null ? property.ToString() : "";
  private static bool Bool(JsonElement value, string name) => value.TryGetProperty(name, out var property) && property.ValueKind == JsonValueKind.True;
  private static long Number(JsonElement value, string name) => long.TryParse(Text(value, name), out var number) ? number : 0;
  private static DateTimeOffset Time(JsonElement value, params string[] names)
  {
    foreach (var name in names) if (DateTimeOffset.TryParse(Text(value, name), out var time)) return time;
    return DateTimeOffset.MinValue;
  }
  private static bool HasLabel(JsonElement value, string? label) => label is not null && value.GetProperty("labels").EnumerateArray().Any(l => Text(l, "name") == label);
  private static FailureReason ReasonFor(Exception exception) => exception switch
  {
    CollectionException known => known.Reason,
    OperationCanceledException => FailureReason.Timeout,
    HttpRequestException => FailureReason.Transport,
    JsonException or KeyNotFoundException or InvalidOperationException => FailureReason.MalformedEvidence,
    _ => FailureReason.SourceUnavailable
  };

  private sealed class CollectionException(string safeMessage, bool partial = false, FailureReason reason = FailureReason.SourceUnavailable) : Exception(safeMessage)
  {
    public string SafeMessage => Message;
    public bool Partial => partial;
    public FailureReason Reason => reason;
  }
}
