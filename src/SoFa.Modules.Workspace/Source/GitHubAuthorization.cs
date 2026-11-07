using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Text.Json;
using Microsoft.AspNetCore.WebUtilities;
using JuanGCarmona.SoFa.Modules.Workspace.Identity;

namespace JuanGCarmona.SoFa.Modules.Workspace.Source;

internal enum ConnectionOutcome { Connected, ConfirmationRequired, Rejected, Unavailable }

internal sealed class GitHubAuthorization(WorkspaceOptions settings, OwnerSessions sessions, GitHubTransport transport, TimeProvider clock)
{
  internal string? Begin(OwnerSession record)
  {
    lock (record.Sync)
    {
      if (!Live(record) || string.IsNullOrEmpty(settings.GitHubClientId)) return null;
      var state = Random();
      var verifier = Random();
      record.GitHub.Replacement = null;
      record.GitHub.Authorization = new GitHubAuthorizationAttempt(state, verifier, ++record.GitHub.Attempt, clock.GetUtcNow().AddMinutes(5));
      return QueryHelpers.AddQueryString("https://github.com/login/oauth/authorize", new Dictionary<string, string?>
      {
        ["client_id"] = settings.GitHubClientId,
        ["redirect_uri"] = Callback,
        ["state"] = state,
        ["code_challenge"] = WebEncoders.Base64UrlEncode(SHA256.HashData(Encoding.ASCII.GetBytes(verifier))),
        ["code_challenge_method"] = "S256",
        ["allow_signup"] = "false",
        ["prompt"] = "select_account",
        ["scope"] = "offline_access",
      });
    }
  }

  internal async Task<ConnectionOutcome> CompleteAsync(OwnerSession record, string state, string? code, CancellationToken cancellationToken)
  {
    GitHubAuthorizationAttempt attempt;
    lock (record.Sync)
    {
      if (!Live(record) || record.GitHub.Authorization is not { } pending || state.Length != 43
          || !CryptographicOperations.FixedTimeEquals(Encoding.ASCII.GetBytes(state), Encoding.ASCII.GetBytes(pending.State)))
        return ConnectionOutcome.Rejected;
      record.GitHub.Authorization = null;
      if (clock.GetUtcNow() >= pending.Deadline || string.IsNullOrEmpty(code) || code.Length > 1024)
        return ConnectionOutcome.Rejected;
      attempt = pending;
    }
    using var exchange = await transport.ExchangeAsync(new Dictionary<string, string>
    {
      ["client_id"] = settings.GitHubClientId!,
      ["client_secret"] = settings.GitHubClientSecret!,
      ["code"] = code,
      ["redirect_uri"] = Callback,
      ["code_verifier"] = attempt.Verifier,
    }, cancellationToken).ConfigureAwait(false);
    var tokens = exchange.Successful ? ParseTokens(exchange.Value) : null;
    if (tokens is null) return ConnectionOutcome.Unavailable;
    lock (record.Sync) if (!Live(record) || record.GitHub.Attempt != attempt.Attempt) return ConnectionOutcome.Rejected;
    using var user = await transport.GetAsync("/user", tokens.Access, cancellationToken).ConfigureAwait(false);
    var id = SourceJson.Number(user.Value, "id");
    var login = SourceJson.Text(user.Value, "login");
    if (!user.Successful || id is not > 0 || SourceJson.Text(user.Value, "type") != "User"
        || login is null || !Regex.IsMatch(login, "^[A-Za-z0-9][A-Za-z0-9-]{0,38}$", RegexOptions.CultureInvariant, TimeSpan.FromMilliseconds(100)))
      return ConnectionOutcome.Unavailable;
    lock (record.Sync)
    {
      if (!Live(record) || record.GitHub.Attempt != attempt.Attempt || clock.GetUtcNow() >= attempt.Deadline)
        return ConnectionOutcome.Rejected;
      var connection = new GitHubConnection(id.Value, login, tokens, clock.GetUtcNow());
      if (record.GitHub.Current is { } current && current.UserId != id)
      {
        record.GitHub.Replacement = connection;
        record.GitHub.ReplacementDeadline = clock.GetUtcNow().AddMinutes(5);
        return ConnectionOutcome.ConfirmationRequired;
      }
      record.GitHub.Current = connection;
      record.GitHub.Observations.Clear();
      record.GitHub.Generation++;
      return ConnectionOutcome.Connected;
    }
  }

  internal bool Confirm(OwnerSession record)
  {
    lock (record.Sync)
    {
      if (!Live(record) || record.GitHub.Replacement is not { } replacement) return false;
      record.GitHub.Replacement = null;
      if (clock.GetUtcNow() >= record.GitHub.ReplacementDeadline) return false;
      record.GitHub.Current = replacement;
      record.GitHub.Observations.Clear();
      record.GitHub.Generation++;
      record.GitHub.Attempt++;
      record.GitHub.Authorization = null;
      return true;
    }
  }

  internal static void Disconnect(OwnerSession record) { lock (record.Sync) record.GitHub.Clear(); }

  internal async Task<GitHubConnection?> CurrentAsync(OwnerSession record, CancellationToken cancellationToken)
  {
    await record.GitHub.RefreshLock.WaitAsync(cancellationToken).ConfigureAwait(false);
    try
    {
      GitHubConnection connection;
      lock (record.Sync)
      {
        if (!Live(record) || record.GitHub.Current is not { } current) return null;
        connection = current;
      }
      var refreshed = false;
      if (clock.GetUtcNow() >= connection.Tokens.AccessDeadline.AddMinutes(-1))
      {
        if (clock.GetUtcNow() >= connection.Tokens.RefreshDeadline) { Invalidate(record, connection); return null; }
        using var refresh = await transport.ExchangeAsync(new Dictionary<string, string>
        {
          ["client_id"] = settings.GitHubClientId!,
          ["client_secret"] = settings.GitHubClientSecret!,
          ["grant_type"] = "refresh_token",
          ["refresh_token"] = connection.Tokens.Refresh,
        }, cancellationToken).ConfigureAwait(false);
        var tokens = refresh.Successful ? ParseTokens(refresh.Value) : null;
        if (tokens is null)
        {
          if (refresh.Status == System.Net.HttpStatusCode.Unauthorized
              || SourceJson.Text(refresh.Value, "error") is "bad_refresh_token" or "invalid_grant")
            Invalidate(record, connection);
          return null;
        }
        lock (record.Sync)
        {
          if (!Current(record, connection)) return null;
          connection.Tokens = tokens;
          refreshed = true;
        }
      }
      if (refreshed || clock.GetUtcNow() >= connection.VerifiedAt.AddMinutes(5))
      {
        using var user = await transport.GetAsync("/user", connection.Tokens.Access, cancellationToken).ConfigureAwait(false);
        var login = SourceJson.Text(user.Value, "login");
        if (!user.Successful || SourceJson.Number(user.Value, "id") != connection.UserId
            || SourceJson.Text(user.Value, "type") != "User" || login is null
            || !Regex.IsMatch(login, "^[A-Za-z0-9][A-Za-z0-9-]{0,38}$", RegexOptions.CultureInvariant, TimeSpan.FromMilliseconds(100)))
        {
          if (user.Status == System.Net.HttpStatusCode.Unauthorized || user.Successful) Invalidate(record, connection);
          return null;
        }
        lock (record.Sync)
        {
          if (!Current(record, connection)) return null;
          connection.VerifiedAt = clock.GetUtcNow();
          connection.Login = login;
        }
      }
      lock (record.Sync) return Current(record, connection) ? connection : null;
    }
    finally { record.GitHub.RefreshLock.Release(); }
  }

  internal static void Invalidate(OwnerSession record, GitHubConnection connection)
  {
    lock (record.Sync) if (ReferenceEquals(record.GitHub.Current, connection)) record.GitHub.Clear();
  }

  private bool Current(OwnerSession record, GitHubConnection connection) => Live(record) && ReferenceEquals(record.GitHub.Current, connection);
  private bool Live(OwnerSession record) => ReferenceEquals(sessions.Get(record.Id), record);
  private string Callback => new Uri(settings.Origin, "/connections/github/callback").AbsoluteUri;
  private static string Random() => WebEncoders.Base64UrlEncode(RandomNumberGenerator.GetBytes(32));

  private GitHubTokens? ParseTokens(JsonElement value)
  {
    var access = SourceJson.Text(value, "access_token");
    var refresh = SourceJson.Text(value, "refresh_token");
    var lifetime = SourceJson.Number(value, "expires_in");
    var refreshLifetime = SourceJson.Number(value, "refresh_token_expires_in");
    if (access is null || refresh is null || access.Length is < 1 or > 4096 || refresh.Length is < 1 or > 4096
        || lifetime is not (> 0 and <= 28800) || refreshLifetime is not (> 0 and <= 15897600)
        || !string.Equals(SourceJson.Text(value, "token_type"), "bearer", StringComparison.OrdinalIgnoreCase)) return null;
    return new GitHubTokens(access, refresh, clock.GetUtcNow().AddSeconds(lifetime.Value), clock.GetUtcNow().AddSeconds(refreshLifetime.Value));
  }
}
