using System.Net;
using System.Text.Json;
using JuanGCarmona.SoFa.Modules.Workspace.Identity;

namespace JuanGCarmona.SoFa.Modules.Workspace.Source;

internal sealed record ControlSourceResult(string Availability, ControlSourceLease? Lease = null, string? Reason = null);
internal sealed record ControlSourceLease(GitHubConnection Connection, ControlAuthority Controls, SourceRequestGuard Guard,
    Func<bool> Live, Func<JsonElement, CancellationToken, Task<ControlCallbackResult>> Callback,
    Func<CancellationToken, Task<ControlSourceResult>> Validate);

internal sealed class SourceControlAccess(WorkspaceOptions settings, OwnerSessions sessions, GitHubAuthorization authorization,
    RepositoryAccess access, GitHubTransport transport)
{
  internal Task<ControlSourceResult> OpenRepository(OwnerSession session, string repository, CancellationToken cancellationToken) =>
      Open(new(session, repository, 1), cancellationToken);

  internal async Task<ControlSourceResult> Open(GetIssueControls query, CancellationToken cancellationToken)
  {
    var record = query.Session;
    ControlSourceResult Failure(string availability, string reason)
    {
      lock (record.Sync) return new(availability, Reason: reason);
    }
    if (query.Number <= 0 || !settings.Repositories.Contains(query.Repository, StringComparer.OrdinalIgnoreCase))
      return Failure("invalid", "Choose a configured repository and positive issue number.");
    if (settings.GitHubAppId is null) return Failure("unsupported", "Configure the intended GitHub App ID before inspecting shared control authority.");
    var connection = await authorization.CurrentAsync(record, cancellationToken).ConfigureAwait(false);
    if (connection is null) return Failure("disconnected", "Connect and verify the acting GitHub user.");
    long generation;
    lock (record.Sync) generation = record.GitHub.Generation;
    bool Live()
    {
      lock (record.Sync) return ReferenceEquals(sessions.Get(record.Id), record)
          && ReferenceEquals(record.GitHub.Current, connection) && record.GitHub.Generation == generation;
    }
    var guard = new SourceRequestGuard(record.Sync, Live, connection.Stopped.Token);
    bool SameScope(RepositoryAuthority value) => value.Controls is { } controls
        && controls.Generation == generation && controls.DataScope == connection.DataScope;
    var authority = await access.CheckControlsAsync(record, query.Repository, cancellationToken).ConfigureAwait(false);
    if (!Live()) return Failure("disconnected", "The source account or owner session changed.");
    if (authority.Availability == SourceAvailability.Denied || authority.Availability == SourceAvailability.Available && !authority.IssuesRead)
      return Failure("denied", "Current connected-user issue authority was denied.");
    if (authority.Availability != SourceAvailability.Available || !SameScope(authority))
      return Failure("unavailable", "The intended App installation and repository identity could not be verified.");
    var controls = authority.Controls!;

    async Task<ControlCallbackResult> Callback(JsonElement value, CancellationToken token)
    {
      if (!Live()) return new(false, Status: 409);
      var kind = SourceJson.Text(value, "kind");
      if (kind == "authority")
      {
        if (!Fields(value, "actorId", "repositoryId") || SourceJson.Number(value, "actorId") != connection.UserId
            || SourceJson.Number(value, "repositoryId") != controls.RepositoryId) throw new InvalidDataException("Invalid source authority callback.");
        var current = await access.CheckControlsAsync(record, query.Repository, token).ConfigureAwait(false);
        if (!Live() || current.Availability != SourceAvailability.Available || !current.IssuesRead || !SameScope(current)
            || current.Controls!.RepositoryId != controls.RepositoryId) return new(false, Status: 403);
        return new(true, new
        {
          actorId = connection.UserId,
          repositoryId = controls.RepositoryId,
          commentWrite = current.Controls.CommentWrite,
          dispatchWrite = current.Controls.DispatchWrite
        });
      }
      if (kind == "artifact")
      {
        if (!Fields(value, "artifactId", "member") || SourceJson.Number(value, "artifactId") is not > 0 or > 9_007_199_254_740_991
            || SourceJson.Text(value, "member") is not ("request.json" or "envelope.json" or "result.json"))
          throw new InvalidDataException("Invalid source artifact callback.");
        if (controls.ActionsRead != true) return new(false, Status: controls.ActionsRead == false ? 403 : 503);
        var content = await transport.ReadArtifactAsync(query.Repository, SourceJson.Number(value, "artifactId")!.Value,
            connection.Tokens.Access, SourceJson.Text(value, "member")!, guard, token).ConfigureAwait(false);
        if (content.Availability == "disconnected") GitHubAuthorization.Invalidate(record, connection);
        return content.Availability == "available" ? new(true, content.Content) : new(false, Status: content.Availability == "disconnected" ? 401 : 503);
      }
      if (kind != "transport" || !Fields(value, "scope", "path") || SourceJson.Text(value, "path") is not { } path)
        throw new InvalidDataException("Invalid source transport callback.");
      string target;
      if (SourceJson.Text(value, "scope") == "account" && path == "user") target = "/user";
      else if (SourceJson.Text(value, "scope") == "repository" && ControlReadPaths.Allowed(path))
      {
        var permission = path.StartsWith("contents/", StringComparison.Ordinal) || path.StartsWith("commits/", StringComparison.Ordinal) ? controls.ContentsRead
            : path.StartsWith("pulls", StringComparison.Ordinal) ? controls.PullRequestsRead
            : path.StartsWith("actions/variables/", StringComparison.Ordinal) ? controls.VariablesRead
            : path.StartsWith("actions/", StringComparison.Ordinal) ? controls.ActionsRead : true;
        var publicRead = permission == false && controls.PublicRepository
            && (path.StartsWith("contents/", StringComparison.Ordinal) || path.StartsWith("commits/", StringComparison.Ordinal)
                || path == "pulls?state=open&per_page=100");
        if (permission != true && !publicRead) return new(false, Status: permission == false ? 403 : 503);
        target = "/repos/" + query.Repository + (path.Length == 0 ? "" : "/" + path);
      }
      else throw new InvalidDataException("Unsupported source read path.");
      using var response = await transport.GetAsync(target, connection.Tokens.Access, guard, token).ConfigureAwait(false);
      if (response.Status == HttpStatusCode.Unauthorized) GitHubAuthorization.Invalidate(record, connection);
      if (!Live()) return new(false, Status: 409);
      if (!response.Successful) return new(false, Status: (int)response.Status);
      if (target == "/user" && (SourceJson.Number(response.Value, "id") != connection.UserId || SourceJson.Text(response.Value, "type") != "User"))
      { GitHubAuthorization.Invalidate(record, connection); return new(false, Status: 401); }
      if (path.Length == 0 && (SourceJson.Number(response.Value, "id") != controls.RepositoryId
          || !string.Equals(SourceJson.Text(response.Value, "full_name"), query.Repository, StringComparison.OrdinalIgnoreCase)))
        return new(false, Status: 409);
      return new(true, response.Value.Clone());
    }


    async Task<ControlSourceResult> Validate(CancellationToken token)
    {
      if (!Live()) return new ControlSourceResult("disconnected", Reason: "The source account or owner session changed.");
      var final = await access.CheckControlsAsync(record, query.Repository, token).ConfigureAwait(false);
      if (!Live()) return new ControlSourceResult("disconnected", Reason: "The source account or owner session changed.");
      if (final.Availability == SourceAvailability.Denied || final.Availability == SourceAvailability.Available && !final.IssuesRead)
        return new ControlSourceResult("denied", Reason: "Current connected-user authority was denied; earlier shared control facts are removed.");
      if (final.Availability != SourceAvailability.Available || !SameScope(final))
        return new ControlSourceResult("unavailable", Reason: "Current source authority could not be reverified.");
      if (final.Controls != controls) return new ControlSourceResult("denied", Reason: "App or repository grants changed during the observation; refresh current source facts.");
      return new("available");
    }
    return new("available", new(connection, controls, guard, Live, Callback, Validate));
  }

  private static bool Fields(JsonElement value, string first, string second) => value.EnumerateObject().Count() == 5
      && value.EnumerateObject().All(property => property.Name is "type" or "id" or "kind" || property.Name == first || property.Name == second);
}
