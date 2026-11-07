using System.Collections.Concurrent;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.AspNetCore.Http;
using JuanGCarmona.SoFa.Modules.Workspace.Source;

namespace JuanGCarmona.SoFa.Modules.Workspace.Identity;

internal sealed class OwnerSessions(IdentityProfile profile, TimeProvider clock) : ITicketStore
{
  internal const string SessionClaim = "sofa:session";
  private readonly ConcurrentDictionary<string, OwnerSession> sessions = new(StringComparer.Ordinal);
  private readonly object admissionLock = new();
  private readonly ConditionalWeakTable<ClaimsPrincipal, OwnerSession> rotations = new();

  public Task<string> StoreAsync(AuthenticationTicket ticket)
  {
    if (rotations.TryGetValue(ticket.Principal, out var original))
    {
      lock (original.Sync)
      {
        rotations.Remove(ticket.Principal);
        if (!ReferenceEquals(Get(original.Id), original)
            || OwnerAdmission.Validate(ticket.Principal, profile, clock.GetUtcNow(), false)?.AuthenticationTime != original.AuthenticationTime)
          throw new InvalidOperationException("The session can no longer change its reference.");
        var previous = original.Id;
        var replacement = WebEncoders.Base64UrlEncode(RandomNumberGenerator.GetBytes(32));
        // A provider refresh can finish after preparation but before issuance.
        // Transfer its latest tokens atomically, never the prepared snapshot's tokens.
        ticket.Properties.StoreTokens(original.Ticket.Properties.GetTokens());
        ticket.Properties.IssuedUtc = original.Ticket.Properties.IssuedUtc;
        SetReference(ticket, replacement);
        ticket.Properties.ExpiresUtc = original.AbsoluteDeadline;
        ticket.Properties.AllowRefresh = false;
        ticket.Properties.IsPersistent = false;
        if (!sessions.TryRemove(previous, out _) || !sessions.TryAdd(replacement, original))
          throw new InvalidOperationException("Could not rotate the session reference.");
        original.Id = replacement;
        original.Ticket = ticket;
        return Task.FromResult(replacement);
      }
    }
    var now = clock.GetUtcNow();
    var admission = OwnerAdmission.Validate(ticket.Principal, profile, now, freshCeremony: true)
        ?? throw new InvalidOperationException("The identity cannot establish an owner session.");
    lock (admissionLock)
    {
      foreach (var key in sessions.Keys) _ = Get(key);
      if (sessions.Count >= 32)
        throw new InvalidOperationException("The development session capacity is full. End an existing session or wait for expiry.");

      var id = WebEncoders.Base64UrlEncode(RandomNumberGenerator.GetBytes(32));
      SetReference(ticket, id);
      ticket.Properties.IssuedUtc = now;
      ticket.Properties.ExpiresUtc = admission.AbsoluteDeadline;
      ticket.Properties.AllowRefresh = false;
      ticket.Properties.IsPersistent = false;
      var record = new OwnerSession(id, ticket, admission.AuthenticationTime,
          admission.AbsoluteDeadline, Earlier(now.AddMinutes(15), admission.AbsoluteDeadline), now);
      if (!sessions.TryAdd(id, record))
        throw new InvalidOperationException("Could not establish a new session reference.");
      return Task.FromResult(id);
    }
  }

  internal async Task RotateCookieAsync(HttpContext context, OwnerSession record)
  {
    ClaimsPrincipal principal;
    AuthenticationProperties properties;
    lock (record.Sync)
    {
      if (!ReferenceEquals(Get(record.Id), record)) throw new InvalidOperationException("The session has ended.");
      principal = new ClaimsPrincipal(record.Ticket.Principal.Identities.Select(identity => new ClaimsIdentity(identity)));
      properties = new AuthenticationProperties(new Dictionary<string, string?>(record.Ticket.Properties.Items));
      rotations.Add(principal, record);
    }
    try { await context.SignInAsync(IdentitySchemes.Issuance, principal, properties).ConfigureAwait(false); }
    finally { rotations.Remove(principal); }
  }

  private static void SetReference(AuthenticationTicket ticket, string reference)
  {
    foreach (var identity in ticket.Principal.Identities)
      foreach (var claim in identity.FindAll(SessionClaim).ToArray()) identity.RemoveClaim(claim);
    ((ClaimsIdentity)ticket.Principal.Identity!).AddClaim(new Claim(SessionClaim, reference));
  }

  public Task<AuthenticationTicket?> RetrieveAsync(string key) => Task.FromResult(Get(key)?.Ticket);

  public Task RenewAsync(string key, AuthenticationTicket ticket)
  {
    var record = Get(key);
    if (record is null) return Task.CompletedTask;
    lock (record.Sync)
    {
      if (record.Id != key || !ReferenceEquals(Get(key), record)) return Task.CompletedTask;
      var admission = OwnerAdmission.Validate(ticket.Principal, profile, clock.GetUtcNow(), freshCeremony: false);
      var identity = ticket.Principal.FindAll(SessionClaim).ToArray();
      if (!record.Active || admission?.AuthenticationTime != record.AuthenticationTime
          || identity.Length != 1 || identity[0].Value != key)
      {
        Remove(record);
        return Task.CompletedTask;
      }
      ticket.Properties.ExpiresUtc = record.AbsoluteDeadline;
      ticket.Properties.AllowRefresh = false;
      record.Ticket = ticket;
    }
    return Task.CompletedTask;
  }

  public Task RemoveAsync(string key)
  {
    Remove(key);
    return Task.CompletedTask;
  }

  internal OwnerSession? Get(ClaimsPrincipal principal)
  {
    var identities = principal.FindAll(SessionClaim).Take(2).ToArray();
    return principal.Identity?.IsAuthenticated == true && identities.Length == 1 ? Get(identities[0].Value) : null;
  }

  internal OwnerSession? Get(string key)
  {
    if (!sessions.TryGetValue(key, out var record)) return null;
    lock (record.Sync)
    {
      if (record.Id != key || !sessions.TryGetValue(key, out var current) || !ReferenceEquals(record, current)) return null;
      var now = clock.GetUtcNow();
      if (record.Active && now < record.IdleDeadline && now < record.AbsoluteDeadline) return record;
      Remove(record);
      return null;
    }
  }

  internal bool TouchUserActivity(string key)
  {
    var record = Get(key);
    if (record is null) return false;
    lock (record.Sync)
    {
      var now = clock.GetUtcNow();
      if (record.Id != key || !record.Active || now >= record.IdleDeadline || now >= record.AbsoluteDeadline) return false;
      record.IdleDeadline = Earlier(now.AddMinutes(15), record.AbsoluteDeadline);
      return true;
    }
  }

  internal void Remove(string key)
  {
    if (!sessions.TryGetValue(key, out var record)) return;
    lock (record.Sync)
      if (record.Id == key) Remove(record);
  }

  internal OwnerSession? GetAuthenticatedRequest(HttpContext context)
  {
    if (!context.Items.TryGetValue(typeof(OwnerSession), out var value) || value is not OwnerSession record) return null;
    lock (record.Sync) return ReferenceEquals(Get(record.Id), record) ? record : null;
  }

  internal void Remove(OwnerSession record)
  {
    lock (record.Sync)
    {
      if (!record.Active) return;
      record.Active = false;
      if (sessions.TryGetValue(record.Id, out var current) && ReferenceEquals(record, current))
        sessions.TryRemove(record.Id, out _);
      record.Ticket.Properties.StoreTokens([]);
      record.GitHub.Clear();
    }
  }

  private static DateTimeOffset Earlier(DateTimeOffset first, DateTimeOffset second) => first < second ? first : second;
}

internal sealed class OwnerSession(string id, AuthenticationTicket ticket, DateTimeOffset authenticationTime,
    DateTimeOffset absoluteDeadline, DateTimeOffset idleDeadline, DateTimeOffset providerValidatedAt)
{
  internal object Sync { get; } = new();
  internal string Id { get; set; } = id;
  internal AuthenticationTicket Ticket { get; set; } = ticket;
  internal DateTimeOffset AuthenticationTime { get; } = authenticationTime;
  internal DateTimeOffset AbsoluteDeadline { get; } = absoluteDeadline;
  internal DateTimeOffset IdleDeadline { get; set; } = idleDeadline;
  internal DateTimeOffset ProviderValidatedAt { get; set; } = providerValidatedAt;
  internal DateTimeOffset ProviderRetryAfter { get; set; }
  internal Task<ProviderAuthority>? ProviderRefresh { get; set; }
  internal bool Active { get; set; } = true;
  internal GitHubSession GitHub { get; } = new();
}
