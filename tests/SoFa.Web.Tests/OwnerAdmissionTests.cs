using System.Globalization;
using System.Security.Claims;
using JuanGCarmona.SoFa.Modules.Workspace.Identity;

namespace SoFa.Web.Tests;

public sealed class OwnerAdmissionTests
{
  private static readonly DateTimeOffset Now = DateTimeOffset.FromUnixTimeSeconds(1_800_000_000);

  [Fact]
  public void OnlyTheExactAuthenticatedOwnerWithFreshMfaIsAdmitted()
  {
    var owner = Principal();
    var admission = OwnerAdmission.Validate(owner, Profile(), Now, freshCeremony: true);
    Assert.NotNull(admission);
    Assert.Equal(Now.AddHours(4), admission.AbsoluteDeadline);
    Assert.Null(OwnerAdmission.Validate(Principal(subject: "another-owner"), Profile(), Now, true));
    Assert.Null(OwnerAdmission.Validate(Principal(issuer: "https://other.invalid/realms/dev"), Profile(), Now, true));
    Assert.Null(OwnerAdmission.Validate(Principal(assurance: "1"), Profile(), Now, true));
    Assert.Null(OwnerAdmission.Validate(new ClaimsPrincipal(new ClaimsIdentity(owner.Claims)), Profile(), Now, true));
  }

  [Fact]
  public void MissingDuplicateAndMalformedIdentityEvidenceFailsClosed()
  {
    foreach (var type in new[] { "iss", "sub", "acr", "auth_time" })
    {
      var claims = Principal().Claims.ToList();
      Assert.Null(OwnerAdmission.Validate(new ClaimsPrincipal(new ClaimsIdentity(claims.Where(c => c.Type != type), "oidc")), Profile(), Now, true));
      claims.Add(claims.Single(c => c.Type == type));
      Assert.Null(OwnerAdmission.Validate(new ClaimsPrincipal(new ClaimsIdentity(claims, "oidc")), Profile(), Now, true));
    }
    Assert.Null(OwnerAdmission.Validate(Principal(authenticationTime: "not-a-time"), Profile(), Now, true));
    Assert.Null(OwnerAdmission.Validate(Principal(authenticationTime: long.MaxValue.ToString(CultureInfo.InvariantCulture)), Profile(), Now, true));
  }

  [Fact]
  public void AProviderSsoCookieCannotStartANewSessionFromAnOldCeremony()
  {
    var old = Principal(authenticationTime: Now.AddMinutes(-6).ToUnixTimeSeconds().ToString(CultureInfo.InvariantCulture));
    Assert.Null(OwnerAdmission.Validate(old, Profile(), Now, freshCeremony: true));
    Assert.NotNull(OwnerAdmission.Validate(old, Profile(), Now, freshCeremony: false));
    Assert.Null(OwnerAdmission.Validate(Principal(authenticationTime: Now.AddHours(-4).ToUnixTimeSeconds().ToString(CultureInfo.InvariantCulture)), Profile(), Now, false));
    Assert.Null(OwnerAdmission.Validate(Principal(authenticationTime: Now.AddSeconds(61).ToUnixTimeSeconds().ToString(CultureInfo.InvariantCulture)), Profile(), Now, true));
  }

  internal static IdentityProfile Profile() => new()
  {
    Issuer = new Uri("https://identity.invalid/realms/dev"),
    ClientId = "sofa-development",
    ClientSecret = "isolated-test-client-value",
    OwnerSubject = "owner-subject",
  };

  internal static ClaimsPrincipal Principal(string issuer = "https://identity.invalid/realms/dev", string subject = "owner-subject", string assurance = "2", string? authenticationTime = null) =>
      new(new ClaimsIdentity(new[]
      {
            new Claim("iss", issuer), new Claim("sub", subject), new Claim("acr", assurance),
            new Claim("auth_time", authenticationTime ?? Now.ToUnixTimeSeconds().ToString(CultureInfo.InvariantCulture)),
            new Claim("email", "same-address@example.invalid"),
      }, "oidc"));
}
