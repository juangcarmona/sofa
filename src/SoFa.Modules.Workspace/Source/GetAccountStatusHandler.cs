using JuanGCarmona.Lighthouse.Cqrs;
using JuanGCarmona.SoFa.Modules.Workspace.Identity;

namespace JuanGCarmona.SoFa.Modules.Workspace.Source;

internal sealed record GetAccountStatus(OwnerSession Session);
internal sealed record RepositoryGrant(string Repository, string Availability, bool IssuesRead, bool ActionsRead, bool IssuesWrite, DateTimeOffset CheckedAt);
internal sealed record AccountStatus(string State, string? Login = null, long? UserId = null,
    DateTimeOffset? VerifiedAt = null, IReadOnlyList<RepositoryGrant>? Repositories = null);

internal sealed class GetAccountStatusHandler(WorkspaceOptions settings, OwnerSessions sessions, GitHubAuthorization authorization,
    RepositoryAccess access) : IQueryHandler<GetAccountStatus, AccountStatus>
{
  public async Task<AccountStatus> Handle(GetAccountStatus query, CancellationToken cancellationToken)
  {
    if (string.IsNullOrEmpty(settings.GitHubClientId)) return new AccountStatus("unconfigured");
    var record = query.Session;
    var connection = await authorization.CurrentAsync(record, cancellationToken).ConfigureAwait(false);
    if (connection is null)
    {
      lock (record.Sync) return new AccountStatus(record.GitHub.Current is null ? "unlinked" : "unavailable");
    }
    long generation;
    lock (record.Sync) generation = record.GitHub.Generation;
    var authorities = await Task.WhenAll(settings.Repositories.Select(repository => access.CheckAsync(record, repository, cancellationToken))).ConfigureAwait(false);
    lock (record.Sync)
    {
      if (!ReferenceEquals(sessions.Get(record.Id), record) || !ReferenceEquals(record.GitHub.Current, connection)
          || record.GitHub.Generation != generation) return new AccountStatus("unavailable");
      return new AccountStatus("linked", connection.Login, connection.UserId, connection.VerifiedAt,
          authorities.Select(authority => new RepositoryGrant(authority.Repository, authority.Availability.ToString().ToLowerInvariant(),
              authority.IssuesRead, authority.ActionsRead, authority.IssuesWrite, authority.CheckedAt)).ToArray());
    }
  }
}
