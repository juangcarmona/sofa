using System.Globalization;
using System.Security.Claims;

namespace JuanGCarmona.SoFa.Modules.Workspace.Identity;

internal sealed record OwnerAdmission(DateTimeOffset AuthenticationTime, DateTimeOffset AbsoluteDeadline)
{
  public static OwnerAdmission? Validate(ClaimsPrincipal principal, IdentityProfile profile,
      DateTimeOffset now, bool freshCeremony)
  {
    if (principal.Identity?.IsAuthenticated != true
        || Single(principal, "iss") != profile.Issuer.AbsoluteUri
        || Single(principal, "sub") != profile.OwnerSubject
        || Single(principal, "acr") != "2"
        || !long.TryParse(Single(principal, "auth_time"), NumberStyles.None,
            CultureInfo.InvariantCulture, out var seconds))
      return null;

    DateTimeOffset authenticatedAt;
    try { authenticatedAt = DateTimeOffset.FromUnixTimeSeconds(seconds); }
    catch (ArgumentOutOfRangeException) { return null; }

    if (authenticatedAt > now.AddSeconds(60) || now >= authenticatedAt.AddHours(4)
        || freshCeremony && authenticatedAt < now.AddMinutes(-5))
      return null;

    return new OwnerAdmission(authenticatedAt, authenticatedAt.AddHours(4));
  }

  private static string? Single(ClaimsPrincipal principal, string type)
  {
    var values = principal.FindAll(type).Take(2).ToArray();
    return values.Length == 1 ? values[0].Value : null;
  }
}
