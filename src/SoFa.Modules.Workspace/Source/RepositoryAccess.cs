using System.Net;
using System.Text.Json;
using JuanGCarmona.SoFa.Modules.Workspace.Identity;

namespace JuanGCarmona.SoFa.Modules.Workspace.Source;

internal enum SourceAvailability { Available, Denied, Unavailable }
internal sealed record RepositoryAuthority(string Repository, SourceAvailability Availability, bool IssuesRead,
    bool ActionsRead, bool IssuesWrite, DateTimeOffset CheckedAt);

internal sealed class RepositoryAccess(WorkspaceOptions settings, OwnerSessions sessions, GitHubAuthorization authorization,
    GitHubTransport transport, TimeProvider clock)
{
  internal async Task<RepositoryAuthority> CheckAsync(OwnerSession record, string repository, CancellationToken cancellationToken)
  {
    var now = clock.GetUtcNow();
    RepositoryAuthority Unknown(SourceAvailability availability) => new(repository, availability, false, false, false, now);
    if (!settings.Repositories.Contains(repository, StringComparer.OrdinalIgnoreCase)) return Unknown(SourceAvailability.Denied);
    var connection = await authorization.CurrentAsync(record, cancellationToken).ConfigureAwait(false);
    if (connection is null) return Unknown(SourceAvailability.Unavailable);
    long generation;
    lock (record.Sync) generation = record.GitHub.Generation;
    using var repo = await transport.GetAsync("/repos/" + repository, connection.Tokens.Access, cancellationToken).ConfigureAwait(false);
    if (!repo.Successful)
    {
      if (repo.Status == HttpStatusCode.Unauthorized) GitHubAuthorization.Invalidate(record, connection);
      return Unknown(repo.Status == HttpStatusCode.NotFound ? SourceAvailability.Denied : SourceAvailability.Unavailable);
    }
    if (repo.Value.ValueKind != JsonValueKind.Object || !repo.Value.TryGetProperty("permissions", out var userPermissions)
        || userPermissions.ValueKind != JsonValueKind.Object || !userPermissions.TryGetProperty("pull", out var pull)
        || pull.ValueKind is not (JsonValueKind.True or JsonValueKind.False)) return Unknown(SourceAvailability.Unavailable);
    if (!Granted(userPermissions, "pull")) return Unknown(SourceAvailability.Denied);

    // Enumerate fixed API paths, never follow a provider-supplied URL with credentials.
    // Explicit bounds produce unknown coverage, never a false permission denial.
    for (var page = 1; page <= 10; page++)
    {
      using var installations = await transport.GetAsync("/user/installations?per_page=100&page="
          + page.ToString(System.Globalization.CultureInfo.InvariantCulture), connection.Tokens.Access, cancellationToken).ConfigureAwait(false);
      if (!installations.Successful || !Array(installations.Value, "installations", out var rows))
      {
        if (installations.Status == HttpStatusCode.Unauthorized) GitHubAuthorization.Invalidate(record, connection);
        return Unknown(SourceAvailability.Unavailable);
      }
      foreach (var installation in rows.EnumerateArray())
      {
        if (installation.ValueKind != JsonValueKind.Object || !installation.TryGetProperty("account", out var account)
            || !string.Equals(SourceJson.Text(account, "login"), repository.Split('/')[0], StringComparison.OrdinalIgnoreCase)) continue;
        if (SourceJson.Number(installation, "id") is not > 0
            || !installation.TryGetProperty("permissions", out var grants)
            || installation.TryGetProperty("suspended_at", out var suspended) && suspended.ValueKind != JsonValueKind.Null)
          return Unknown(SourceAvailability.Unavailable);
        var id = SourceJson.Number(installation, "id")!.Value;
        for (var repoPage = 1; repoPage <= 50; repoPage++)
        {
          using var accessible = await transport.GetAsync("/user/installations/" + id.ToString(System.Globalization.CultureInfo.InvariantCulture)
              + "/repositories?per_page=100&page=" + repoPage.ToString(System.Globalization.CultureInfo.InvariantCulture), connection.Tokens.Access, cancellationToken).ConfigureAwait(false);
          if (!accessible.Successful || !Array(accessible.Value, "repositories", out var repositories))
          {
            if (accessible.Status == HttpStatusCode.Unauthorized) GitHubAuthorization.Invalidate(record, connection);
            return Unknown(SourceAvailability.Unavailable);
          }
          foreach (var item in repositories.EnumerateArray())
          {
            if (!string.Equals(SourceJson.Text(item, "full_name"), repository, StringComparison.OrdinalIgnoreCase)) continue;
            lock (record.Sync)
            {
              if (!ReferenceEquals(sessions.Get(record.Id), record) || !ReferenceEquals(record.GitHub.Current, connection)
                  || record.GitHub.Generation != generation) return Unknown(SourceAvailability.Unavailable);
              var metadata = Permission(grants, "metadata");
              var issues = Permission(grants, "issues");
              var actions = Permission(grants, "actions");
              var write = Granted(userPermissions, "push") || Granted(userPermissions, "maintain") || Granted(userPermissions, "admin");
              return new RepositoryAuthority(repository, metadata ? SourceAvailability.Available : SourceAvailability.Denied,
                  metadata && issues, metadata && actions, metadata && write && SourceJson.Text(grants, "issues") == "write", now);
            }
          }
          if (Complete(accessible.Value, repositories.GetArrayLength(), repoPage)) return Unknown(SourceAvailability.Denied);
        }
        return Unknown(SourceAvailability.Unavailable);
      }
      if (Complete(installations.Value, rows.GetArrayLength(), page)) return Unknown(SourceAvailability.Denied);
    }
    return Unknown(SourceAvailability.Unavailable);
  }

  private static bool Complete(JsonElement root, int length, int page) => length < 100
      && SourceJson.Number(root, "total_count") is { } total && total >= 0 && total <= (page - 1) * 100 + length;
  private static bool Array(JsonElement value, string name, out JsonElement array)
  {
    array = default;
    return value.ValueKind == JsonValueKind.Object && value.TryGetProperty(name, out array) && array.ValueKind == JsonValueKind.Array;
  }
  private static bool Permission(JsonElement value, string name) => SourceJson.Text(value, name) is "read" or "write";
  private static bool Granted(JsonElement value, string name) => value.ValueKind == JsonValueKind.Object
      && value.TryGetProperty(name, out var permission) && permission.ValueKind == JsonValueKind.True;
}
