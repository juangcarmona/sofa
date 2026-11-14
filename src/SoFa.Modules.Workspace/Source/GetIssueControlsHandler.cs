using System.Net;
using System.Text.Json;
using System.Text.RegularExpressions;
using JuanGCarmona.Lighthouse.Cqrs;
using JuanGCarmona.SoFa.Modules.Workspace.Identity;

namespace JuanGCarmona.SoFa.Modules.Workspace.Source;

internal sealed record GetIssueControls(OwnerSession Session, string Repository, int Number);
internal sealed record IssueControlObservation(string Repository, int Number, string Availability, DateTimeOffset ObservedAt,
    string? DataScope = null, long? ActingUserId = null, JsonElement? Facts = null, string? Reason = null,
    IReadOnlyList<string>? MissingReadScopes = null, bool SubmissionAvailable = false);

internal sealed class GetIssueControlsHandler(SourceControlAccess source, SourceControlBridge bridge, ControlJournal journal, TimeProvider clock)
    : IQueryHandler<GetIssueControls, IssueControlObservation>
{
  public async Task<IssueControlObservation> Handle(GetIssueControls query, CancellationToken cancellationToken)
  {
    IssueControlObservation Failure(string availability, string? reason)
    {
      lock (query.Session.Sync) return new(query.Repository, query.Number, availability, clock.GetUtcNow(),
          query.Session.GitHub.Current?.DataScope, query.Session.GitHub.Current?.UserId, Reason: reason);
    }
    using var budget = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
    budget.CancelAfter(TimeSpan.FromSeconds(20));
    var opened = await source.Open(query, budget.Token).ConfigureAwait(false);
    if (opened.Lease is not { } lease) return Failure(opened.Availability, opened.Reason);
    var connection = lease.Connection;
    var controls = lease.Controls;
    var observation = await bridge.Observe(query.Repository, query.Number, lease.Callback, budget.Token).ConfigureAwait(false);
    var final = await lease.Validate(budget.Token).ConfigureAwait(false);
    if (final.Availability != "available") return Failure(final.Availability, final.Reason);
    if (observation.Availability == "available" && (observation.Facts is not { } facts
        || !facts.TryGetProperty("actor", out var actor) || SourceJson.Number(actor, "id") != connection.UserId
        || !facts.TryGetProperty("repository", out var repo) || SourceJson.Number(repo, "id") != controls.RepositoryId
        || !string.Equals(SourceJson.Text(repo, "name"), query.Repository, StringComparison.OrdinalIgnoreCase)
        || !facts.TryGetProperty("issue", out var issue) || SourceJson.Number(issue, "number") != query.Number))
      return Failure("unavailable", "The shared observation did not preserve its acting user and source identity.");
    var missing = new[] { ("contents", controls.ContentsRead), ("pull_requests", controls.PullRequestsRead),
      ("variables", controls.VariablesRead), ("actions", controls.ActionsRead) }.Where(item => item.Item2 != true).Select(item => item.Item1).ToArray();
    return new(query.Repository, query.Number, observation.Availability, clock.GetUtcNow(), connection.DataScope,
        connection.UserId, observation.Facts, observation.Reason, missing, journal.IsAvailable);
  }

}
internal static class ControlReadPaths
{
  private const string Reads = @"^(?:issues/[1-9][0-9]*(?:/comments\?per_page=100&page=[1-9][0-9]*)?|issues/comments/[1-9][0-9]*|pulls\?state=open&per_page=100|pulls/[1-9][0-9]*(?:/(?:reviews|commits|files)\?per_page=100(?:&page=[1-9][0-9]*)?)?|commits/[^/?#\\\s]{1,768}|contents/(?:lighthouse\.json|package(?:-lock)?\.json|\.lighthouse/(?:upgrade|sdlc|commands)\.json|\.lighthouse/(?:reconciliation/)?[a-z0-9_.-]+\.json|\.github/workflows/[A-Za-z0-9_.-]+\.(?:yml|md)|openspec/changes/issue-[1-9][0-9]*-proposal)\?ref=[a-f0-9]{40}|actions/variables/LIGHTHOUSE_(?:AUTOMATION_ENABLED|EPIC_ENABLED)|actions/runs/[1-9][0-9]*(?:/attempts/[1-9][0-9]*)?(?:/(?:jobs|artifacts)\?per_page=100(?:&page=[1-9][0-9]*)?)?|actions/artifacts\?name=lighthouse-[a-z0-9-]{1,190}&per_page=100|actions/workflows/[A-Za-z0-9_.-]+\.yml/runs\?status=(?:queued|in_progress|waiting|requested|pending)&per_page=100)$";
  internal static bool Allowed(string path)
  {
    if (path.Length == 0) return true;
    if (path.Length > 2048 || path.Any(character => char.IsControl(character) || character == '\\') || path.StartsWith('/') || path.Contains('#')) return false;
    try
    {
      var decoded = Uri.UnescapeDataString(Uri.UnescapeDataString(path));
      if (decoded.Contains("..", StringComparison.Ordinal) || decoded.Contains('\\') || decoded.Any(char.IsControl)) return false;
      return Regex.IsMatch(path, Reads, RegexOptions.CultureInvariant | RegexOptions.NonBacktracking, TimeSpan.FromMilliseconds(100));
    }
    catch (UriFormatException) { return false; }
  }
}
