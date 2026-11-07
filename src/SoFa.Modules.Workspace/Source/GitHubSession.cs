namespace JuanGCarmona.SoFa.Modules.Workspace.Source;

internal sealed class GitHubSession
{
  internal long Generation { get; set; }
  internal long Attempt { get; set; }
  internal GitHubAuthorizationAttempt? Authorization { get; set; }
  internal GitHubConnection? Current { get; set; }
  internal GitHubConnection? Replacement { get; set; }
  internal DateTimeOffset ReplacementDeadline { get; set; }
  internal SemaphoreSlim RefreshLock { get; } = new(1, 1);
  internal Dictionary<string, CachedObservation> Observations { get; } = new(StringComparer.OrdinalIgnoreCase);

  internal void Clear()
  {
    Generation++;
    Attempt++;
    Authorization = null;
    Current = null;
    Replacement = null;
    Observations.Clear();
  }
}

internal sealed class GitHubAuthorizationAttempt(string state, string verifier, long attempt, DateTimeOffset deadline)
{
  internal string State { get; } = state;
  internal string Verifier { get; } = verifier;
  internal long Attempt { get; } = attempt;
  internal DateTimeOffset Deadline { get; } = deadline;
  public override string ToString() => "GitHub authorization attempt (protocol values withheld)";
}

internal sealed class GitHubConnection(long userId, string login, GitHubTokens tokens, DateTimeOffset verifiedAt)
{
  internal long UserId { get; } = userId;
  internal string Login { get; set; } = login;
  internal GitHubTokens Tokens { get; set; } = tokens;
  internal DateTimeOffset VerifiedAt { get; set; } = verifiedAt;
  public override string ToString() => "GitHub connection (credentials withheld)";
}

internal sealed class GitHubTokens(string access, string refresh, DateTimeOffset accessDeadline, DateTimeOffset refreshDeadline)
{
  internal string Access { get; } = access;
  internal string Refresh { get; } = refresh;
  internal DateTimeOffset AccessDeadline { get; } = accessDeadline;
  internal DateTimeOffset RefreshDeadline { get; } = refreshDeadline;
  public override string ToString() => "GitHub tokens (credentials withheld)";
}
