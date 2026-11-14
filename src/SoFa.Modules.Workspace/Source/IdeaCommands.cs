using System.Net;
using System.Text.Json;
using JuanGCarmona.SoFa.Modules.Workspace.Identity;

namespace JuanGCarmona.SoFa.Modules.Workspace.Source;

internal sealed record IdeaCommand(string Operation, string? Id = null, int Revision = 0, string? Repository = null,
    string? Title = null, string? Outcome = null, string? Scope = null, int? Number = null);
internal sealed record RelatedIdeaIssue(int Number, string Title, string Url, bool Equivalent);
internal sealed record IdeaResult(string Availability, IdeaDraft[] Ideas, string[] Repositories, string? Reason = null,
    RelatedIdeaIssue[]? Related = null, string? Preview = null, string? ActingUser = null);

internal sealed class IdeaCommands(WorkspaceOptions settings, SourceControlAccess source, ControlJournal journal, GitHubTransport transport)
{
  private string Owner => settings.Identity.Issuer.AbsoluteUri + "#" + settings.Identity.OwnerSubject;
  internal IdeaResult List()
  {
    var result = journal.Ideas(Owner);
    return new(result.State, result.Prepared?.Deserialize<IdeaDraft[]>() ?? [], settings.Repositories.ToArray());
  }

  internal async Task<IdeaResult> Handle(OwnerSession session, IdeaCommand command, CancellationToken cancellationToken)
  {
    IdeaResult Fail(string state, string reason) => List() with { Availability = state, Reason = reason };
    if (command.Operation is not ("save" or "abandon" or "review" or "create" or "link" or "reconcile")
        || command.Id is not null && !Guid.TryParseExact(command.Id, "N", out _)) return Fail("invalid", "Choose a supported idea action.");
    var list = List();
    if (list.Availability != "available") return Fail("unavailable", "Protected draft storage is unavailable. Configure the existing control journal.");
    var draft = list.Ideas.SingleOrDefault(value => value.Id == command.Id);
    if (command.Id is not null && draft is null) return Fail("invalid", "The draft was not found.");
    if (draft is not null && command.Revision != draft.Revision) return Fail("conflict", "The draft changed. Refresh before continuing.");
    // Confirmed source identity is immutable, including across repeated submissions.
    if (draft?.Source is not null) return list;
    IdeaResult Save(IdeaDraft value)
    {
      var saved = journal.Ideas(Owner, value with { Revision = (draft?.Revision ?? 0) + 1 }, draft?.Revision ?? 0);
      return saved.State == "available" ? List() : Fail(saved.State, "The draft changed or protected storage is unavailable. Refresh before continuing.");
    }
    if (command.Operation == "save")
    {
      if (draft?.State is "uncertain" or "abandoned") return Fail("conflict", "This retained draft cannot be edited or resent.");
      if (command.Title is not { Length: > 0 and <= 200 } || string.IsNullOrWhiteSpace(command.Title)
          || command.Outcome is not { Length: > 0 and <= 4000 } || string.IsNullOrWhiteSpace(command.Outcome)
          || command.Scope is not { Length: > 0 and <= 4000 } || string.IsNullOrWhiteSpace(command.Scope)
          || !settings.Repositories.Contains(command.Repository, StringComparer.OrdinalIgnoreCase))
        return Fail("invalid", "Enter a title, intended outcome, scope and configured repository within the field limits.");
      return Save(new(draft?.Id ?? Guid.NewGuid().ToString("N"), 0, settings.Repositories.First(value => value.Equals(command.Repository, StringComparison.OrdinalIgnoreCase)),
          command.Title.Trim(), command.Outcome, command.Scope));
    }
    if (draft is null) return Fail("invalid", "Save the idea first.");
    if (command.Operation == "abandon") return draft.State == "uncertain"
        ? Fail("conflict", "Reconcile the uncertain creation before abandoning it.") : Save(draft with { State = "abandoned" });
    if (draft.State == "abandoned") return Fail("conflict", "This idea was abandoned; no source operation is allowed.");
    if (draft.State == "uncertain" && command.Operation != "reconcile") return Fail("conflict", "Creation is uncertain. Reconcile it; do not resend.");
    using var budget = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
    budget.CancelAfter(TimeSpan.FromSeconds(25));
    var opened = await source.OpenRepository(session, draft.Repository, budget.Token).ConfigureAwait(false);
    if (opened.Lease is not { } lease) return Fail(opened.Availability, opened.Reason ?? "Source authority is unavailable.");
    using var user = await transport.GetAsync("/user", lease.Connection.Tokens.Access, lease.Guard, budget.Token).ConfigureAwait(false);
    if (!user.Successful || SourceJson.Number(user.Value, "id") != lease.Connection.UserId || SourceJson.Text(user.Value, "type") != "User")
      return Fail("denied", "The current GitHub user could not be verified. Reconnect GitHub.");
    if (command.Operation is ("create" or "reconcile" or "link")
        && (draft.ActorId != lease.Connection.UserId || draft.RepositoryId != lease.Controls.RepositoryId))
      return Fail("conflict", "The reviewed GitHub actor or repository changed. Review the candidates again.");
    if (command.Operation == "link")
    {
      if (draft.State != "reviewed") return Fail("conflict", "Review related issues before linking an existing issue.");
      if (command.Number is not > 0) return Fail("invalid", "Choose a positive existing issue number.");
      using var issue = await transport.GetAsync("/repos/" + draft.Repository + "/issues/" + command.Number, lease.Connection.Tokens.Access, lease.Guard, budget.Token).ConfigureAwait(false);
      var identity = issue.Successful ? Identity(issue.Value, draft.Repository, lease.Controls.RepositoryId) : null;
      var current = await lease.Validate(budget.Token).ConfigureAwait(false);
      return identity is not null && identity.Number == command.Number && current.Availability == "available"
          ? Save(draft with { Source = identity, State = "linked", ActorId = lease.Connection.UserId, RepositoryId = lease.Controls.RepositoryId })
          : Fail("unavailable", "The existing issue identity or current read authority could not be verified.");
    }
    var scan = await Scan(draft, lease, budget.Token).ConfigureAwait(false);
    if (scan is null) return Fail("unavailable", "Issue coverage is incomplete or unavailable (maximum 1,000 records). Creation remains blocked; reconcile later.");
    var related = scan.Where(value => Related(draft.Title, SourceJson.Text(value, "title")!)).Select(value => new RelatedIdeaIssue(
        (int)SourceJson.Number(value, "number")!.Value, SourceJson.Text(value, "title")!, Identity(value, draft.Repository, lease.Controls.RepositoryId)!.Url,
        Normalize(SourceJson.Text(value, "title")!) == Normalize(draft.Title))).ToArray();
    if (command.Operation == "reconcile")
    {
      if (draft.State != "uncertain") return Fail("conflict", "There is no uncertain creation to reconcile.");
      var matches = scan.Where(value => SourceJson.Text(value, "body") == Body(draft)
          && value.TryGetProperty("user", out var author) && SourceJson.Number(author, "id") == draft.ActorId).ToArray();
      var verified = await lease.Validate(budget.Token).ConfigureAwait(false);
      if (verified.Availability != "available") return Fail(verified.Availability, verified.Reason!);
      return matches.Length == 1 ? Save(draft with { Source = Identity(matches[0], draft.Repository, lease.Controls.RepositoryId), State = "created" })
          : Fail("conflict", "No unique matching source issue is confirmed. The uncertain intent is retained; no retry was sent.");
    }
    if (command.Operation == "review")
      return Save(draft with { State = "reviewed", ActorId = lease.Connection.UserId, RepositoryId = lease.Controls.RepositoryId }) with
      { Related = related, Preview = Body(draft), ActingUser = lease.Connection.Login, Reason = "Nothing has been sent. Inspect candidates and exact content before confirming creation." };
    if (lease.Controls.CommentWrite != true) return Fail("denied", "Current GitHub user and App issue-write permission is required. The draft is retained.");
    if (draft.State != "reviewed") return Fail("conflict", "Review the saved idea before confirming creation.");
    if (related.Any(value => value.Equivalent)) return Fail("conflict", "An equivalent title already exists. Link the existing issue instead of creating another.") with { Related = related };
    var final = await lease.Validate(budget.Token).ConfigureAwait(false);
    if (final.Availability != "available") return Fail(final.Availability, final.Reason!);
    var claim = Save(draft with { State = "uncertain" });
    if (claim.Availability != "available") return claim;
    // The persisted state is irreversible for sending, even when the request is cancelled before transmission.
    try
    {
      using var response = await transport.PostControlAsync(draft.Repository, "issues", JsonSerializer.SerializeToElement(new { title = draft.Title, body = Body(draft) }),
          null, lease.Connection.Tokens.Access, lease.Guard, budget.Token).ConfigureAwait(false);
      var identity = response.Status == HttpStatusCode.Created ? Identity(response.Value, draft.Repository, lease.Controls.RepositoryId) : null;
      if (!lease.Live() || identity is null) return Fail("unavailable", "Creation is uncertain. Reconcile the retained intent; do not submit another issue.");
      draft = draft with { Revision = draft.Revision + 1, State = "uncertain" };
      return Save(draft with { Source = identity, State = "created" });
    }
    catch (OperationCanceledException) { return Fail("unavailable", "Creation is uncertain. Reconcile the retained intent; no retry is allowed."); }
  }

  internal static string Marker(IdeaDraft draft) => "<!-- control-center-idea:" + draft.Id + " -->";
  internal static string Body(IdeaDraft draft) => "## Intended outcome\n\n" + draft.Outcome + "\n\n## Scope\n\n" + draft.Scope + "\n\n" + Marker(draft);
  private static string Normalize(string text) => string.Join(' ', text.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries)).ToUpperInvariant();
  private static bool Related(string title, string candidate) => Normalize(title) == Normalize(candidate)
      || Normalize(title).Split(' ').Any(word => word.Length > 2 && Normalize(candidate).Contains(word, StringComparison.Ordinal));
  private static IdeaSource? Identity(JsonElement issue, string repository, long repositoryId)
  {
    var id = SourceJson.Number(issue, "id"); var number = SourceJson.Number(issue, "number");
    if (id is not (> 0 and <= 9_007_199_254_740_991) || number is not (> 0 and <= int.MaxValue)
        || issue.TryGetProperty("pull_request", out _) || SourceJson.Text(issue, "title") is not { Length: > 0 and <= 1024 }) return null;
    var expected = "https://github.com/" + repository + "/issues/" + number.Value.ToString(System.Globalization.CultureInfo.InvariantCulture);
    return SourceJson.Text(issue, "html_url") == expected ? new(repositoryId, id.Value, (int)number.Value, expected) : null;
  }
  private async Task<List<JsonElement>?> Scan(IdeaDraft draft, ControlSourceLease lease, CancellationToken token)
  {
    var rows = new List<JsonElement>();
    for (var page = 1; page <= 10; page++)
    {
      using var response = await transport.GetAsync("/repos/" + draft.Repository + "/issues?state=all&per_page=100&page=" + page,
          lease.Connection.Tokens.Access, lease.Guard, token).ConfigureAwait(false);
      if (!response.Successful || !lease.Live() || response.Value.ValueKind != JsonValueKind.Array || response.Value.GetArrayLength() > 100) return null;
      foreach (var value in response.Value.EnumerateArray())
      {
        if (value.TryGetProperty("pull_request", out _)) continue;
        if (Identity(value, draft.Repository, lease.Controls.RepositoryId) is null) return null;
        rows.Add(value.Clone());
      }
      if (response.Value.GetArrayLength() < 100) return rows;
    }
    return null;
  }
}
