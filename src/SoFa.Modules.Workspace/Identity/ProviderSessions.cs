using System.Globalization;
using System.Net;
using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;

namespace JuanGCarmona.SoFa.Modules.Workspace.Identity;

internal enum ProviderAuthority { Current, Unavailable, Revoked }

internal static class IdentitySchemes
{
  internal const string Cookie = "sofa-owner";
  internal const string Issuance = "sofa-owner-issuance";
  internal const string Oidc = "sofa-oidc";
}

internal sealed class ProviderSessions(OwnerSessions sessions, IdentityProfile profile,
    IOptionsMonitor<OpenIdConnectOptions> options, TimeProvider clock)
{
  internal Task<ProviderAuthority> EnsureCurrentAsync(OwnerSession record, CancellationToken cancellationToken)
  {
    Task<ProviderAuthority> refresh;
    lock (record.Sync)
    {
      if (!ReferenceEquals(sessions.Get(record.Id), record)) return Task.FromResult(ProviderAuthority.Revoked);
      var now = clock.GetUtcNow();
      if (now < record.ProviderValidatedAt.AddMinutes(5)) return Task.FromResult(ProviderAuthority.Current);
      if (now < record.ProviderRetryAfter) return Task.FromResult(ProviderAuthority.Unavailable);
      // Coalesce an online refresh; a completed failure can be retried after its bounded delay.
      if (record.ProviderRefresh is null || record.ProviderRefresh.IsCompleted)
        record.ProviderRefresh = RefreshAsync(record);
      refresh = record.ProviderRefresh;
    }
    return refresh.WaitAsync(cancellationToken);
  }

  private async Task<ProviderAuthority> RefreshAsync(OwnerSession record)
  {
    try
    {
      string? previousRefresh;
      lock (record.Sync) previousRefresh = record.Ticket.Properties.GetTokenValue("refresh_token");
      if (string.IsNullOrEmpty(previousRefresh)) return Revoke(record);
      var oidc = options.Get(IdentitySchemes.Oidc);
      OpenIdConnectConfiguration configuration;
      try
      {
        configuration = await oidc.ConfigurationManager!.GetConfigurationAsync(CancellationToken.None).ConfigureAwait(false);
      }
      catch (InvalidOperationException) { return Unavailable(record); }
      if (configuration.Issuer != profile.Issuer.AbsoluteUri
          || !Uri.TryCreate(configuration.TokenEndpoint, UriKind.Absolute, out var endpoint)
          || endpoint.Scheme != "https" || endpoint.UserInfo.Length != 0 || endpoint.Fragment.Length != 0)
        return Unavailable(record);

      using var body = new FormUrlEncodedContent(new Dictionary<string, string>
      {
        ["grant_type"] = "refresh_token",
        ["refresh_token"] = previousRefresh,
        ["client_id"] = profile.ClientId,
        ["client_secret"] = profile.ClientSecret,
      });
      using var response = await oidc.Backchannel.PostAsync(endpoint, body, CancellationToken.None).ConfigureAwait(false);
      await response.Content.LoadIntoBufferAsync(64 * 1024).ConfigureAwait(false);
      using var payload = JsonDocument.Parse(await response.Content.ReadAsStringAsync(CancellationToken.None).ConfigureAwait(false));
      var value = payload.RootElement;
      if (response.StatusCode == HttpStatusCode.BadRequest && Text(value, "error") == "invalid_grant") return Revoke(record);
      if (!response.IsSuccessStatusCode) return Unavailable(record);

      var idToken = Text(value, "id_token");
      var accessToken = Text(value, "access_token");
      if (string.IsNullOrEmpty(idToken) || string.IsNullOrEmpty(accessToken)
          || value.ValueKind != JsonValueKind.Object
          || !value.TryGetProperty("expires_in", out var expiry) || expiry.ValueKind != JsonValueKind.Number
          || !expiry.TryGetInt32(out var seconds)
          || seconds is < 1 or > 86400)
        return Unavailable(record);

      if (value.TryGetProperty("refresh_token", out var refreshValue)
          && (refreshValue.ValueKind != JsonValueKind.String || string.IsNullOrEmpty(refreshValue.GetString())))
        return Unavailable(record);

      var validation = oidc.TokenValidationParameters.Clone();
      validation.ValidateIssuer = true;
      validation.ValidIssuer = profile.Issuer.AbsoluteUri;
      validation.ValidIssuers = null;
      validation.ValidateAudience = true;
      validation.ValidAudience = profile.ClientId;
      validation.ValidAudiences = null;
      validation.ValidateLifetime = true;
      validation.RequireExpirationTime = true;
      validation.RequireSignedTokens = true;
      validation.ValidateIssuerSigningKey = true;
      validation.ClockSkew = TimeSpan.FromSeconds(60);
      if (oidc.ConfigurationManager is BaseConfigurationManager manager) validation.ConfigurationManager = manager;
      else validation.IssuerSigningKeys = configuration.SigningKeys;
      var validated = await oidc.TokenHandler.ValidateTokenAsync(idToken, validation).ConfigureAwait(false);
      if (!validated.IsValid || validated.ClaimsIdentity is null) return Revoke(record);
      var principal = new ClaimsPrincipal(validated.ClaimsIdentity);
      var admission = OwnerAdmission.Validate(principal, profile, clock.GetUtcNow(), freshCeremony: false);
      var refreshedNonce = principal.FindAll("nonce").Take(2).ToArray();
      if (admission?.AuthenticationTime != record.AuthenticationTime
          || refreshedNonce.Length > 1 || refreshedNonce.Length == 1
              && refreshedNonce[0].Value != record.Ticket.Principal.FindFirst("nonce")?.Value)
        return Revoke(record);

      lock (record.Sync)
      {
        if (!ReferenceEquals(sessions.Get(record.Id), record)) return ProviderAuthority.Revoked;
        record.Ticket.Properties.StoreTokens(new[]
        {
                    new AuthenticationToken { Name = "id_token", Value = idToken },
                    new AuthenticationToken { Name = "access_token", Value = accessToken },
                    new AuthenticationToken { Name = "refresh_token", Value = Text(value, "refresh_token") ?? previousRefresh },
                    new AuthenticationToken { Name = "expires_at", Value = clock.GetUtcNow().AddSeconds(seconds).ToString("o", CultureInfo.InvariantCulture) },
                });
        record.ProviderValidatedAt = clock.GetUtcNow();
        record.ProviderRetryAfter = default;
      }
      return ProviderAuthority.Current;
    }
    catch (HttpRequestException) { return Unavailable(record); }
    catch (OperationCanceledException) { return Unavailable(record); }
    catch (JsonException) { return Unavailable(record); }
  }

  private ProviderAuthority Revoke(OwnerSession record)
  {
    sessions.Remove(record);
    return ProviderAuthority.Revoked;
  }

  private ProviderAuthority Unavailable(OwnerSession record)
  {
    lock (record.Sync)
    {
      if (!ReferenceEquals(sessions.Get(record.Id), record)) return ProviderAuthority.Revoked;
      record.ProviderRetryAfter = clock.GetUtcNow().AddSeconds(5);
    }
    return ProviderAuthority.Unavailable;
  }

  private static string? Text(JsonElement value, string name) => value.ValueKind == JsonValueKind.Object
      && value.TryGetProperty(name, out var property) && property.ValueKind == JsonValueKind.String ? property.GetString() : null;
}
