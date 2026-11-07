using System.Net;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using JuanGCarmona.SoFa.Modules.Workspace.Identity;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;

namespace SoFa.Web.Tests;

public sealed class ProviderSessionsTests
{
  [Fact]
  public async Task DueValidationIsSharedAndCannotRenewTheOriginalDeadlines()
  {
    using var fixture = new ProviderFixture();
    var release = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
    fixture.Handler.Respond = async () => { await release.Task; return fixture.Success(); };
    var record = await fixture.Session();
    var deadline = record.IdleDeadline;
    fixture.Clock.Advance(TimeSpan.FromMinutes(5));
    var calls = Enumerable.Range(0, 10).Select(_ => fixture.Provider.EnsureCurrentAsync(record, CancellationToken.None)).ToArray();
    Assert.Equal(1, fixture.Handler.Calls);
    release.SetResult();
    Assert.All(await Task.WhenAll(calls), result => Assert.Equal(ProviderAuthority.Current, result));
    Assert.Equal(deadline, record.IdleDeadline);
    Assert.Equal(fixture.Clock.Start.AddHours(4), record.AbsoluteDeadline);
    Assert.Equal(fixture.Clock.GetUtcNow(), record.ProviderValidatedAt);
    Assert.Equal("rotated-refresh-test-value", record.Ticket.Properties.GetTokenValue("refresh_token"));
    Assert.Equal(ProviderAuthority.Current, await fixture.Provider.EnsureCurrentAsync(record, CancellationToken.None));
    Assert.Equal(1, fixture.Handler.Calls);
  }

  [Fact]
  public async Task ProviderOutageBlocksAccessWithoutExtendingOrDeletingTheSession()
  {
    using var fixture = new ProviderFixture();
    fixture.Handler.Respond = () => throw new HttpRequestException("isolated provider failure");
    var record = await fixture.Session();
    var original = record.ProviderValidatedAt;
    fixture.Clock.Advance(TimeSpan.FromMinutes(5));
    Assert.Equal(ProviderAuthority.Unavailable, await fixture.Provider.EnsureCurrentAsync(record, CancellationToken.None));
    Assert.Same(record, fixture.Sessions.Get(record.Id));
    Assert.Equal(original, record.ProviderValidatedAt);
    Assert.Equal(ProviderAuthority.Unavailable, await fixture.Provider.EnsureCurrentAsync(record, CancellationToken.None));
    Assert.Equal(1, fixture.Handler.Calls);
    fixture.Clock.Advance(TimeSpan.FromMinutes(10));
    Assert.Equal(ProviderAuthority.Revoked, await fixture.Provider.EnsureCurrentAsync(record, CancellationToken.None));
    Assert.Null(fixture.Sessions.Get(record.Id));
  }

  [Fact]
  public async Task RecognizedRevocationImmediatelyDestroysLocalAuthority()
  {
    using var fixture = new ProviderFixture();
    fixture.Handler.Respond = () => Task.FromResult(new HttpResponseMessage(HttpStatusCode.BadRequest)
    { Content = new StringContent("{\"error\":\"invalid_grant\"}", Encoding.UTF8, "application/json") });
    var record = await fixture.Session();
    fixture.Clock.Advance(TimeSpan.FromMinutes(5));
    Assert.Equal(ProviderAuthority.Revoked, await fixture.Provider.EnsureCurrentAsync(record, CancellationToken.None));
    Assert.Null(fixture.Sessions.Get(record.Id));
    Assert.Null(record.Ticket.Properties.GetTokenValue("refresh_token"));
  }

  [Theory]
  [InlineData("{}")]
  [InlineData("[]")]
  [InlineData("{\"id_token\":\"placeholder\",\"access_token\":\"placeholder\",\"expires_in\":\"300\"}")]
  public async Task MalformedProviderEvidenceIsUnavailableAndCannotRefreshAuthority(string body)
  {
    using var fixture = new ProviderFixture();
    fixture.Handler.Respond = () => Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
    { Content = new StringContent(body, Encoding.UTF8, "application/json") });
    var record = await fixture.Session();
    fixture.Clock.Advance(TimeSpan.FromMinutes(5));
    Assert.Equal(ProviderAuthority.Unavailable, await fixture.Provider.EnsureCurrentAsync(record, CancellationToken.None));
    Assert.Same(record, fixture.Sessions.Get(record.Id));
    Assert.Equal(fixture.Clock.Start, record.ProviderValidatedAt);
  }

  [Theory]
  [InlineData("subject")]
  [InlineData("assurance")]
  [InlineData("audience")]
  [InlineData("expiry")]
  [InlineData("authentication-time")]
  [InlineData("signature")]
  public async Task RefreshedIdentityIsCryptographicallyValidatedAndCannotChangeTheOriginalCeremony(string invalid)
  {
    using var fixture = new ProviderFixture();
    fixture.Handler.Respond = () => Task.FromResult(fixture.Success(invalid));
    var record = await fixture.Session();
    fixture.Clock.Advance(TimeSpan.FromMinutes(5));
    Assert.Equal(ProviderAuthority.Revoked, await fixture.Provider.EnsureCurrentAsync(record, CancellationToken.None));
    Assert.Null(fixture.Sessions.Get(record.Id));
  }

  [Fact]
  public async Task ARefreshAlreadyInFlightCannotRestoreALoggedOutSession()
  {
    using var fixture = new ProviderFixture();
    var release = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
    fixture.Handler.Respond = async () => { await release.Task; return fixture.Success(); };
    var record = await fixture.Session();
    fixture.Clock.Advance(TimeSpan.FromMinutes(5));
    var pending = fixture.Provider.EnsureCurrentAsync(record, CancellationToken.None);
    fixture.Sessions.Remove(record.Id);
    release.SetResult();
    Assert.Equal(ProviderAuthority.Revoked, await pending);
    Assert.Null(record.Ticket.Properties.GetTokenValue("refresh_token"));
  }

  private sealed class ProviderFixture : IDisposable
  {
    private readonly ECDsa signingKey = ECDsa.Create(ECCurve.NamedCurves.nistP256);
    private readonly ECDsa wrongKey = ECDsa.Create(ECCurve.NamedCurves.nistP256);
    private readonly HttpClient client;
    internal TestClock Clock { get; } = new(DateTimeOffset.FromUnixTimeSeconds(DateTimeOffset.UtcNow.ToUnixTimeSeconds()));
    internal RefreshHandler Handler { get; } = new();
    internal OwnerSessions Sessions { get; }
    internal ProviderSessions Provider { get; }

    internal ProviderFixture()
    {
      var profile = OwnerAdmissionTests.Profile();
      Sessions = new OwnerSessions(profile, Clock);
      client = new HttpClient(Handler) { Timeout = TimeSpan.FromSeconds(10) };
      var metadata = new OpenIdConnectConfiguration
      { Issuer = profile.Issuer.AbsoluteUri, TokenEndpoint = profile.Issuer.AbsoluteUri + "/token" };
      metadata.SigningKeys.Add(new ECDsaSecurityKey(signingKey) { KeyId = "provider-key" });
      var options = new OpenIdConnectOptions
      {
        ClientId = profile.ClientId,
        ClientSecret = profile.ClientSecret,
        Backchannel = client,
        ConfigurationManager = new StaticConfigurationManager<OpenIdConnectConfiguration>(metadata),
        TokenHandler = new JsonWebTokenHandler { MapInboundClaims = false },
      };
      Provider = new ProviderSessions(Sessions, profile, new FixedOptions(options), Clock);
    }

    internal async Task<OwnerSession> Session()
    {
      var identity = OwnerAdmissionTests.Principal(authenticationTime: Clock.Start.ToUnixTimeSeconds().ToString(System.Globalization.CultureInfo.InvariantCulture));
      var ticket = new AuthenticationTicket(identity, "owner");
      ticket.Properties.StoreTokens(new[] { new AuthenticationToken { Name = "refresh_token", Value = "original-refresh-test-value" } });
      return Sessions.Get(await Sessions.StoreAsync(ticket))!;
    }

    internal HttpResponseMessage Success(string? invalid = null)
    {
      var key = new ECDsaSecurityKey(invalid == "signature" ? wrongKey : signingKey) { KeyId = "provider-key" };
      var token = new JsonWebTokenHandler().CreateToken(new SecurityTokenDescriptor
      {
        Issuer = OwnerAdmissionTests.Profile().Issuer.AbsoluteUri,
        Audience = invalid == "audience" ? "another-client" : "sofa-development",
        NotBefore = DateTime.UtcNow.AddMinutes(-20),
        Expires = invalid == "expiry" ? DateTime.UtcNow.AddMinutes(-10) : DateTime.UtcNow.AddMinutes(20),
        Subject = new ClaimsIdentity(new[]
          {
                    new Claim("sub", invalid == "subject" ? "different-owner" : "owner-subject"),
                    new Claim("acr", invalid == "assurance" ? "1" : "2"),
                    new Claim("auth_time", (invalid == "authentication-time" ? Clock.Start.AddMinutes(1) : Clock.Start)
                        .ToUnixTimeSeconds().ToString(System.Globalization.CultureInfo.InvariantCulture)),
                }),
        SigningCredentials = new SigningCredentials(key, SecurityAlgorithms.EcdsaSha256),
      });
      return new HttpResponseMessage(HttpStatusCode.OK)
      {
        Content = new StringContent(JsonSerializer.Serialize(new
        {
          access_token = "refreshed-access-test-value",
          refresh_token = "rotated-refresh-test-value",
          id_token = token,
          expires_in = 300,
        }), Encoding.UTF8, "application/json"),
      };
    }

    public void Dispose() { client.Dispose(); signingKey.Dispose(); wrongKey.Dispose(); }
  }

  private sealed class RefreshHandler : HttpMessageHandler
  {
    internal int Calls;
    internal Func<Task<HttpResponseMessage>> Respond { get; set; } = () => throw new InvalidOperationException("Configure the test provider.");
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
      Interlocked.Increment(ref Calls);
      Assert.Equal(HttpMethod.Post, request.Method);
      Assert.Equal("https://identity.invalid/realms/dev/token", request.RequestUri!.AbsoluteUri);
      return Respond();
    }
  }

  private sealed class FixedOptions(OpenIdConnectOptions value) : IOptionsMonitor<OpenIdConnectOptions>
  {
    public OpenIdConnectOptions CurrentValue => value;
    public OpenIdConnectOptions Get(string? name) => value;
    public IDisposable? OnChange(Action<OpenIdConnectOptions, string?> listener) => null;
  }
}
