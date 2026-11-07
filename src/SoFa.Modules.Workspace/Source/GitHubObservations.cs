using System.Globalization;
using System.Text.Json;
using JuanGCarmona.Lighthouse.Cqrs;
using JuanGCarmona.SoFa.Modules.Workspace.Identity;

namespace JuanGCarmona.SoFa.Modules.Workspace.Source;

internal sealed record ObservedRun(long Id, string? Name, string? Status, string? Conclusion, string? SourceRevision,
    string? Url, string? StartedAt, string? UpdatedAt);
internal sealed record ObservedIssue(int Number, string? Title, IReadOnlyList<string> Labels, string? Url, string? UpdatedAt);
internal sealed record ObservedRepository(string Repository, DateTimeOffset ObservedAt, string Availability, string Scope,
    IReadOnlyList<ObservedRun>? Runs = null, IReadOnlyList<ObservedIssue>? Attention = null, string? Reason = null);
internal sealed record ObservationSnapshot(IReadOnlyList<ObservedRepository> Repositories, int RefreshAfterSeconds);
internal sealed record CachedObservation(GitHubConnection Connection, long Generation, Task<ObservedRepository> Collection, DateTimeOffset ExpiresAt);

internal sealed record GetObservations(OwnerSession Session);

internal sealed class GetObservationsHandler(WorkspaceOptions settings, OwnerSessions sessions, GitHubAuthorization authorization,
    RepositoryAccess access, GitHubTransport transport, TimeProvider clock) : IQueryHandler<GetObservations, ObservationSnapshot?>
{
  private const string Scope = "Latest 20 workflow runs and first 100 open issue records; not a complete delivery audit";

  public async Task<ObservationSnapshot?> Handle(GetObservations query, CancellationToken cancellationToken)
  {
    var record = query.Session;
    var connection = await authorization.CurrentAsync(record, cancellationToken).ConfigureAwait(false);
    if (connection is null) return null;
    long generation;
    lock (record.Sync) generation = record.GitHub.Generation;
    var repositories = await Task.WhenAll(settings.Repositories.Select(repository =>
        ReadRepositoryAsync(record, connection, generation, repository, cancellationToken))).ConfigureAwait(false);
    lock (record.Sync)
      return Live(record, connection, generation) ? new ObservationSnapshot(repositories, 15) : null;
  }

  private async Task<ObservedRepository> ReadRepositoryAsync(OwnerSession record, GitHubConnection connection,
      long generation, string repository, CancellationToken cancellationToken)
  {
    // Permission verification precedes every cache lookup, including within the data TTL.
    var authority = await access.CheckAsync(record, repository, cancellationToken).ConfigureAwait(false);
    if (authority.Availability != SourceAvailability.Available || !authority.IssuesRead || !authority.ActionsRead)
    {
      lock (record.Sync) record.GitHub.Observations.Remove(repository);
      return Unavailable(repository);
    }
    Task<ObservedRepository> collection;
    lock (record.Sync)
    {
      if (!Live(record, connection, generation)) return Unavailable(repository);
      if (!record.GitHub.Observations.TryGetValue(repository, out var cached)
          || !ReferenceEquals(cached.Connection, connection) || cached.Generation != generation || clock.GetUtcNow() >= cached.ExpiresAt)
      {
        // Collection has its own short budget: one cancelled browser cannot cancel another browser's shared read.
        collection = CollectAsync(record, connection, generation, repository);
        record.GitHub.Observations[repository] = new CachedObservation(connection, generation, collection, clock.GetUtcNow().AddSeconds(15));
      }
      else collection = cached.Collection;
    }
    var observed = await collection.WaitAsync(cancellationToken).ConfigureAwait(false);
    lock (record.Sync) return Live(record, connection, generation) ? observed : Unavailable(repository);
  }

  private async Task<ObservedRepository> CollectAsync(OwnerSession record, GitHubConnection connection, long generation, string repository)
  {
    using var budget = new CancellationTokenSource(TimeSpan.FromSeconds(20));
    var observedAt = clock.GetUtcNow();
    try
    {
      lock (record.Sync) if (!Live(record, connection, generation)) return Unavailable(repository);
      using var runs = await transport.GetAsync("/repos/" + repository + "/actions/runs?per_page=20", connection.Tokens.Access, budget.Token).ConfigureAwait(false);
      lock (record.Sync) if (!Live(record, connection, generation)) return Unavailable(repository);
      using var issues = await transport.GetAsync("/repos/" + repository + "/issues?state=open&per_page=100", connection.Tokens.Access, budget.Token).ConfigureAwait(false);
      if (runs.Status == System.Net.HttpStatusCode.Unauthorized || issues.Status == System.Net.HttpStatusCode.Unauthorized)
        GitHubAuthorization.Invalidate(record, connection);
      if (!runs.Successful || !issues.Successful || runs.Value.ValueKind != JsonValueKind.Object
          || !runs.Value.TryGetProperty("workflow_runs", out var runRows) || runRows.ValueKind != JsonValueKind.Array
          || issues.Value.ValueKind != JsonValueKind.Array) return Unavailable(repository);
      var mappedRuns = runRows.EnumerateArray().Take(20).Where(r => SourceJson.Number(r, "id") is > 0 and <= 9_007_199_254_740_991)
          .Select(r => new ObservedRun(SourceJson.Number(r, "id")!.Value, Text(r, "name", 256), Text(r, "status", 64),
              Text(r, "conclusion", 64), Text(r, "head_sha", 128), Text(r, "html_url", 2048),
              Timestamp(r, "run_started_at"), Timestamp(r, "updated_at"))).ToArray();
      var mappedIssues = issues.Value.EnumerateArray().Take(100)
          .Where(i => i.ValueKind == JsonValueKind.Object && !i.TryGetProperty("pull_request", out _) && SourceJson.Number(i, "number") is > 0 and <= int.MaxValue)
          .Select(i => new ObservedIssue((int)SourceJson.Number(i, "number")!.Value, Text(i, "title", 1024), Labels(i), Text(i, "html_url", 2048), Timestamp(i, "updated_at")))
          .Where(i => i.Labels.Any(l => l is "needs-info" or "agent-working" or "ready" or "waiting-review")).ToArray();
      return new ObservedRepository(repository, observedAt, "available", Scope, mappedRuns, mappedIssues);
    }
    catch (OperationCanceledException) { return Unavailable(repository); }
  }

  private bool Live(OwnerSession record, GitHubConnection connection, long generation) => ReferenceEquals(sessions.Get(record.Id), record)
      && ReferenceEquals(record.GitHub.Current, connection) && record.GitHub.Generation == generation;
  private ObservedRepository Unavailable(string repository) => new(repository, clock.GetUtcNow(), "unavailable", Scope,
      Reason: "GitHub authority or evidence could not be verified; activity and backlog are unknown.");
  private static string? Text(JsonElement value, string name, int maximum)
  {
    var text = SourceJson.Text(value, name);
    return text?.Length <= maximum ? text : null;
  }
  private static string? Timestamp(JsonElement value, string name) => DateTimeOffset.TryParse(Text(value, name, 64),
      CultureInfo.InvariantCulture, DateTimeStyles.None, out var time) ? time.ToUniversalTime().ToString("o", CultureInfo.InvariantCulture) : null;
  private static string[] Labels(JsonElement issue) => issue.TryGetProperty("labels", out var labels) && labels.ValueKind == JsonValueKind.Array
      ? labels.EnumerateArray().Take(100).Select(l => Text(l, "name", 256)).OfType<string>().ToArray() : [];
}
