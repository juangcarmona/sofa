using System.Globalization;
using System.Net;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using JuanGCarmona.SoFa.Modules.Workspace.Identity;
using JuanGCarmona.SoFa.Modules.Workspace.Source;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;

namespace SoFa.Web.Tests;

public sealed class OwnerBoundaryTests
{
  [Fact]
  public async Task AnonymousRoutesAndCsrfFailuresCannotExposeOperationalDataOrStartLogin()
  {
    await using var app = new OwnerApplication();
    using var client = app.Client();
    foreach (var path in new[] { "/workspace", "/api/session", "/api/observations", "/unknown" })
      Assert.Equal(HttpStatusCode.Unauthorized, (await client.GetAsync(path)).StatusCode);
    Assert.Equal(HttpStatusCode.BadRequest, (await client.PostAsync("/auth/login", new StringContent(""))).StatusCode);
    Assert.Equal(HttpStatusCode.Unauthorized, (await client.PostAsync("/auth/logout", new StringContent(""))).StatusCode);
    using var insecure = app.CreateClient(new WebApplicationFactoryClientOptions { BaseAddress = new Uri("http://workspace.invalid"), AllowAutoRedirect = false });
    Assert.Equal(HttpStatusCode.BadRequest, (await insecure.GetAsync("/")).StatusCode);
    using var wrongHost = app.CreateClient(new WebApplicationFactoryClientOptions { BaseAddress = new Uri("https://other.invalid"), AllowAutoRedirect = false });
    Assert.Equal(HttpStatusCode.BadRequest, (await wrongHost.GetAsync("/")).StatusCode);
    Assert.Equal(0, app.TokenExchanges);
  }

  [Fact]
  public async Task ExpiredFrameworkNonceCannotEstablishAnOwnerSessionEvenWithAValidCodeAndSignature()
  {
    await using var app = new OwnerApplication { NonceLifetime = TimeSpan.FromSeconds(1) };
    using var client = app.Client();
    var callback = await app.Begin(client);
    await Task.Delay(TimeSpan.FromMilliseconds(1300));
    var response = await client.GetAsync(callback);
    Assert.Equal("/?access=denied", response.Headers.Location?.OriginalString);
    Assert.Equal(HttpStatusCode.Unauthorized, (await client.GetAsync("/api/session")).StatusCode);
    Assert.Equal(1, app.TokenExchanges);
    var navigation = await client.GetAsync("/workspace");
    Assert.Contains("Sign in required", await navigation.Content.ReadAsStringAsync(), StringComparison.Ordinal);
    Assert.DoesNotContain("owner-subject", await navigation.Content.ReadAsStringAsync(), StringComparison.Ordinal);
  }

  [Fact]
  public async Task RealMiddlewareRequiresPkceFreshMfaAndAValidSingleUseCallbackBeforeCreatingAnOpaqueCookie()
  {
    await using var app = new OwnerApplication();
    using var client = app.Client();
    var callback = await app.Begin(client);
    var response = await client.GetAsync(callback);
    Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
    Assert.Equal("/workspace", response.Headers.Location?.OriginalString);
    var cookie = Assert.Single(response.Headers.GetValues("Set-Cookie"), c => c.StartsWith("__Host-SoFa.Owner=", StringComparison.Ordinal));
    Assert.Contains("secure", cookie, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("httponly", cookie, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("samesite=lax", cookie, StringComparison.OrdinalIgnoreCase);
    Assert.DoesNotContain("domain=", cookie, StringComparison.OrdinalIgnoreCase);
    Assert.DoesNotContain("access-test-value", cookie, StringComparison.Ordinal);
    Assert.DoesNotContain("owner-subject", cookie, StringComparison.Ordinal);
    var before = await client.GetStringAsync("/api/session");
    Assert.DoesNotContain("token", before, StringComparison.OrdinalIgnoreCase);
    Assert.DoesNotContain("owner-subject", before, StringComparison.Ordinal);
    Assert.Equal(before, await client.GetStringAsync("/api/session"));
    var replay = await client.GetAsync(callback);
    Assert.Equal("/?access=denied", replay.Headers.Location?.OriginalString);
    Assert.Equal(1, app.TokenExchanges);
  }

  [Theory]
  [InlineData("subject")]
  [InlineData("issuer")]
  [InlineData("assurance")]
  [InlineData("nonce")]
  [InlineData("signature")]
  [InlineData("old-ceremony")]
  public async Task InvalidProviderIdentityNeverCreatesAnOwnerSession(string invalid)
  {
    await using var app = new OwnerApplication { Invalid = invalid };
    using var client = app.Client();
    var response = await client.GetAsync(await app.Begin(client));
    Assert.Equal("/?access=denied", response.Headers.Location?.OriginalString);
    Assert.Equal(HttpStatusCode.Unauthorized, (await client.GetAsync("/api/session")).StatusCode);
    Assert.False(response.Headers.TryGetValues("Set-Cookie", out var cookies)
        && cookies.Any(c => c.StartsWith("__Host-SoFa.Owner=", StringComparison.Ordinal)));
  }

  [Fact]
  public async Task ForgedOrAnotherBrowserCallbackCannotRedeemTheCode()
  {
    await using var app = new OwnerApplication();
    using var client = app.Client();
    var callback = await app.Begin(client);
    using var stranger = app.Client();
    Assert.Equal("/?access=denied", (await stranger.GetAsync(callback)).Headers.Location?.OriginalString);
    Assert.Equal("/?access=denied", (await client.GetAsync("/signin-oidc?code=invalid&state=invalid")).Headers.Location?.OriginalString);
    Assert.Equal(0, app.TokenExchanges);
  }

  [Fact]
  public async Task FreshLoginInvalidatesThePreviousBrowserReference()
  {
    await using var app = new OwnerApplication();
    using var client = app.Client();
    var first = await client.GetAsync(await app.Begin(client));
    var oldCookie = OwnerCookie(first);
    var next = await client.GetAsync(await app.Begin(client));
    Assert.NotEqual(oldCookie, OwnerCookie(next));
    Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/api/session")).StatusCode);
    using var stale = app.Client();
    stale.DefaultRequestHeaders.Add("Cookie", oldCookie);
    Assert.Equal(HttpStatusCode.Unauthorized, (await stale.GetAsync("/api/session")).StatusCode);
  }

  [Fact]
  public async Task AccountTransitionRotatesTheOpaqueReferenceWithoutExtendingSessionAuthority()
  {
    await using var app = new OwnerApplication();
    using var client = app.Client();
    var login = await client.GetAsync(await app.Begin(client));
    var oldCookie = OwnerCookie(login);
    using var scope = app.Services.CreateScope();
    var context = new DefaultHttpContext { RequestServices = scope.ServiceProvider };
    context.Request.Scheme = "https";
    context.Request.Host = new HostString("workspace.invalid");
    context.Request.Headers.Cookie = oldCookie;
    var authenticated = await context.AuthenticateAsync(IdentitySchemes.Cookie);
    var sessions = scope.ServiceProvider.GetRequiredService<OwnerSessions>();
    var record = sessions.Get(authenticated.Principal!)!;
    var previousId = record.Id;
    var absolute = record.AbsoluteDeadline;
    var idle = record.IdleDeadline;
    var validated = record.ProviderValidatedAt;
    app.Clock.Advance(TimeSpan.FromMinutes(6));
    app.BeforeIssue = () =>
    {
      lock (record.Sync) record.Ticket.Properties.StoreTokens(new[]
              { new AuthenticationToken { Name = "refresh_token", Value = "provider-rotated-before-reference-change" } });
    };
    await sessions.RotateCookieAsync(context, record);
    Assert.Equal("provider-rotated-before-reference-change", record.Ticket.Properties.GetTokenValue("refresh_token"));
    var nextCookie = Assert.Single(context.Response.Headers.SetCookie.ToArray(), c => c!.StartsWith("__Host-SoFa.Owner=", StringComparison.Ordinal))!.Split(';')[0];
    Assert.NotEqual(previousId, record.Id);
    Assert.Equal(previousId, authenticated.Principal!.FindFirst(OwnerSessions.SessionClaim)!.Value);
    Assert.Null(sessions.Get(previousId));
    Assert.Same(record, sessions.Get(record.Id));
    Assert.Equal(absolute, record.AbsoluteDeadline);
    Assert.Equal(idle, record.IdleDeadline);
    Assert.Equal(validated, record.ProviderValidatedAt);
    using var stale = app.Client();
    stale.DefaultRequestHeaders.Add("Cookie", oldCookie);
    Assert.Equal(HttpStatusCode.Unauthorized, (await stale.GetAsync("/api/session")).StatusCode);
    using var rotated = app.Client();
    rotated.DefaultRequestHeaders.Add("Cookie", nextCookie);
    // The rotated reference is valid, but overdue provider validation must still block it.
    Assert.Equal(HttpStatusCode.ServiceUnavailable, (await rotated.GetAsync("/api/session")).StatusCode);
  }

  [Fact]
  public async Task LogoutRequiresCsrfAndDestroysTheServerTicketEvenWhenTheProviderIsUnavailable()
  {
    await using var app = new OwnerApplication();
    using var client = app.Client();
    await client.GetAsync(await app.Begin(client));
    Assert.Equal(HttpStatusCode.BadRequest, (await client.PostAsync("/auth/logout", new StringContent(""))).StatusCode);
    var page = await client.GetStringAsync("/workspace");
    var token = Token(page);
    app.Clock.Advance(TimeSpan.FromMinutes(5));
    Assert.Equal(HttpStatusCode.ServiceUnavailable, (await client.GetAsync("/api/session")).StatusCode);
    // This is a server invalidation, not a request to end the provider's browser SSO.
    var response = await client.PostAsync("/auth/logout", new FormUrlEncodedContent(new Dictionary<string, string> { ["__RequestVerificationToken"] = token }));
    Assert.Equal("/", response.Headers.Location?.OriginalString);
    Assert.Equal(HttpStatusCode.Unauthorized, (await client.GetAsync("/api/session")).StatusCode);
    Assert.Equal(1, app.TokenExchanges);
  }

  [Fact]
  public async Task SourceConnectionAndDisconnectRequireCsrfRotateReferencesAndNeverMutateIssuesOrWorkflows()
  {
    await using var app = new OwnerApplication();
    using var client = app.Client();
    var login = await client.GetAsync(await app.Begin(client));
    var previousCookie = OwnerCookie(login);
    Assert.Equal(HttpStatusCode.BadRequest, (await client.PostAsync("/connections/github/start", new StringContent(""))).StatusCode);
    Assert.Empty(app.Source.Calls);
    var token = Token(await client.GetStringAsync("/workspace"));
    var begin = await client.PostAsync("/connections/github/start", Form(token));
    Assert.Equal(HttpStatusCode.Redirect, begin.StatusCode);
    var query = QueryHelpers.ParseQuery(begin.Headers.Location!.Query);
    app.Source.ExpectedChallenge = query["code_challenge"].ToString();
    var callback = QueryHelpers.AddQueryString("/connections/github/callback", new Dictionary<string, string?>
    { ["state"] = query["state"], ["code"] = "isolated-code" });
    var linked = await client.GetAsync(callback);
    Assert.Equal("/workspace?connection=connected", linked.Headers.Location?.OriginalString);
    Assert.NotEqual(previousCookie, OwnerCookie(linked));
    var page = await client.GetStringAsync("/workspace");
    Assert.Contains("verified-user", page, StringComparison.Ordinal);
    Assert.DoesNotContain("isolated-user-access", page, StringComparison.Ordinal);
    Assert.DoesNotContain("isolated-user-refresh", page, StringComparison.Ordinal);
    Assert.Equal("/workspace?connection=rejected", (await client.GetAsync(callback)).Headers.Location?.OriginalString);
    Assert.Equal(HttpStatusCode.BadRequest, (await client.PostAsync("/connections/github/disconnect", new StringContent(""))).StatusCode);
    var disconnected = await client.PostAsync("/connections/github/disconnect", Form(Token(page)));
    Assert.NotEqual(OwnerCookie(linked), OwnerCookie(disconnected));
    Assert.Contains("GitHub is not connected.", await client.GetStringAsync("/workspace"), StringComparison.Ordinal);
    Assert.Equal(2, app.Source.Calls.Count);
  }

  private static FormUrlEncodedContent Form(string token) => new(new Dictionary<string, string> { ["__RequestVerificationToken"] = token });

  [Fact]
  public async Task LogoutStillDestroysAuthorityIfItsAuthenticatedReferenceRotatesBeforeTheRouteRuns()
  {
    await using var app = new OwnerApplication();
    using var client = app.Client();
    await client.GetAsync(await app.Begin(client));
    var token = Token(await client.GetStringAsync("/workspace"));
    OwnerSession? rotated = null;
    app.OnValidated = async context =>
    {
      if (context.Request.Path != "/auth/logout") return;
      var sessions = context.HttpContext.RequestServices.GetRequiredService<OwnerSessions>();
      rotated = sessions.Get(context.Principal!)!;
      await sessions.RotateCookieAsync(context.HttpContext, rotated);
    };
    var response = await client.PostAsync("/auth/logout", Form(token));
    Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
    Assert.NotNull(rotated);
    Assert.False(rotated.Active);
    Assert.Null(rotated.Ticket.Properties.GetTokenValue("refresh_token"));
    Assert.Equal(HttpStatusCode.Unauthorized, (await client.GetAsync("/api/session")).StatusCode);
  }

  [Fact]
  public async Task AuthenticatedObserverServesExistingAssetsAndLosesCachedDataWhenSourcePermissionIsRemoved()
  {
    await using var app = new OwnerApplication();
    using var client = app.Client();
    await client.GetAsync(await app.Begin(client));
    var token = Token(await client.GetStringAsync("/workspace"));
    var begin = await client.PostAsync("/connections/github/start", Form(token));
    var query = QueryHelpers.ParseQuery(begin.Headers.Location!.Query);
    app.Source.ExpectedChallenge = query["code_challenge"].ToString();
    await client.GetAsync(QueryHelpers.AddQueryString("/connections/github/callback", new Dictionary<string, string?>
    { ["state"] = query["state"], ["code"] = "isolated-code" }));
    var operations = await client.GetStringAsync("/workspace/operations");
    Assert.Contains("Factory operations", operations, StringComparison.Ordinal);
    using var anonymous = app.Client();
    Assert.Equal(HttpStatusCode.OK, (await anonymous.GetAsync("/app.js")).StatusCode);
    Assert.Equal(HttpStatusCode.OK, (await anonymous.GetAsync("/tokens.css")).StatusCode);
    using var style = await anonymous.GetAsync("/account.css");
    Assert.Equal("text/css", style.Content.Headers.ContentType?.MediaType);
    Assert.Contains(".account-panel", await style.Content.ReadAsStringAsync(), StringComparison.Ordinal);
    using var script = await anonymous.GetAsync("/account.js");
    Assert.Equal("text/javascript", script.Content.Headers.ContentType?.MediaType);
    Assert.Contains("/api/connections/github", await script.Content.ReadAsStringAsync(), StringComparison.Ordinal);
    using var authority = JsonDocument.Parse(await client.GetStringAsync("/api/connections/github"));
    Assert.Equal("linked", authority.RootElement.GetProperty("state").GetString());
    Assert.Equal(71, authority.RootElement.GetProperty("userId").GetInt64());
    Assert.True(Assert.Single(authority.RootElement.GetProperty("repositories").EnumerateArray()).GetProperty("issuesWrite").GetBoolean());
    using var data = JsonDocument.Parse(await client.GetStringAsync("/api/observations"));
    var repository = Assert.Single(data.RootElement.GetProperty("repositories").EnumerateArray());
    Assert.Equal("available", repository.GetProperty("availability").GetString());
    Assert.Equal("observed-revision", Assert.Single(repository.GetProperty("runs").EnumerateArray()).GetProperty("sourceRevision").GetString());
    Assert.Equal(1, Assert.Single(repository.GetProperty("attention").EnumerateArray()).GetProperty("number").GetInt32());
    app.Source.CanRead = false;
    using var denied = JsonDocument.Parse(await client.GetStringAsync("/api/observations"));
    var unavailable = Assert.Single(denied.RootElement.GetProperty("repositories").EnumerateArray());
    Assert.Equal("unavailable", unavailable.GetProperty("availability").GetString());
    Assert.Equal(JsonValueKind.Null, unavailable.GetProperty("runs").ValueKind);
    var account = await client.GetStringAsync("/workspace");
    await client.PostAsync("/connections/github/disconnect", Form(Token(account)));
    var calls = app.Source.Calls.Count;
    Assert.Equal(HttpStatusCode.Conflict, (await client.GetAsync("/api/observations")).StatusCode);
    Assert.Equal(calls, app.Source.Calls.Count);
    using var disconnected = JsonDocument.Parse(await client.GetStringAsync("/api/connections/github"));
    Assert.Equal("unlinked", disconnected.RootElement.GetProperty("state").GetString());
    Assert.Equal(JsonValueKind.Null, disconnected.RootElement.GetProperty("login").ValueKind);
  }

  private static string Token(string html) => WebUtility.HtmlDecode(Regex.Match(html,
      "name=\"__RequestVerificationToken\" value=\"([^\"]+)\"", RegexOptions.CultureInvariant, TimeSpan.FromSeconds(1)).Groups[1].Value);

  private static string OwnerCookie(HttpResponseMessage response) =>
      Assert.Single(response.Headers.GetValues("Set-Cookie"), c => c.StartsWith("__Host-SoFa.Owner=", StringComparison.Ordinal)).Split(';')[0];

  private sealed class OwnerApplication : WebApplicationFactory<Program>
  {
    private readonly ECDsa key = ECDsa.Create(ECCurve.NamedCurves.nistP256);
    private string nonce = "";
    internal string? Invalid { get; init; }
    internal int TokenExchanges;
    internal TimeSpan? NonceLifetime { get; init; }
    internal Action? BeforeIssue { get; set; }
    internal Func<CookieValidatePrincipalContext, Task>? OnValidated { get; set; }
    internal GitHubAuthorizationTests.SourceHandler Source { get; } = new();
    internal TestClock Clock { get; } = new(DateTimeOffset.FromUnixTimeSeconds(DateTimeOffset.UtcNow.ToUnixTimeSeconds()));

    internal HttpClient Client() => CreateClient(new WebApplicationFactoryClientOptions
    { BaseAddress = new Uri("https://workspace.invalid"), AllowAutoRedirect = false, HandleCookies = true });

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
      builder.ConfigureAppConfiguration((_, configuration) => configuration.AddInMemoryCollection(new Dictionary<string, string?>
      {
        ["SoFa:Origin"] = "https://workspace.invalid",
        ["SoFa:Identity:Mode"] = "Development",
        ["SoFa:Identity:Development:Issuer"] = "https://identity.invalid/realms/dev",
        ["SoFa:Identity:Development:ClientId"] = "sofa-development",
        ["SoFa:Identity:Development:ClientSecret"] = "isolated-test-client-value",
        ["SoFa:Identity:Development:OwnerSubject"] = "owner-subject",
        ["SoFa:Repositories:0"] = "example/repository",
        ["SoFa:GitHub:ClientId"] = "development-user-app",
        ["SoFa:GitHub:ClientSecret"] = "isolated-user-app-value",
      }));
      builder.ConfigureServices(services =>
      {
        services.AddSingleton<TimeProvider>(Clock);
        services.AddSingleton(_ => new GitHubTransport(Source));
        services.PostConfigure<CookieAuthenticationOptions>(IdentitySchemes.Issuance, options =>
                  options.Events.OnSigningIn = _ => { BeforeIssue?.Invoke(); return Task.CompletedTask; });
        services.PostConfigure<OpenIdConnectOptions>(IdentitySchemes.Oidc, ConfigureProvider);
        services.PostConfigure<CookieAuthenticationOptions>(IdentitySchemes.Cookie, ConfigureOwnerCookie);
      });
    }

    private void ConfigureOwnerCookie(CookieAuthenticationOptions options)
    {
      var validate = options.Events.OnValidatePrincipal;
      options.Events.OnValidatePrincipal = async context =>
      {
        await validate(context);
        if (context.Principal is not null && OnValidated is not null) await OnValidated(context);
      };
    }

    private void ConfigureProvider(OpenIdConnectOptions options)
    {
      if (NonceLifetime is { } lifetime) options.ProtocolValidator.NonceLifetime = lifetime;
      var metadata = new OpenIdConnectConfiguration
      {
        Issuer = "https://identity.invalid/realms/dev",
        AuthorizationEndpoint = "https://identity.invalid/authorize",
        TokenEndpoint = "https://identity.invalid/token",
      };
      metadata.SigningKeys.Add(new ECDsaSecurityKey(key) { KeyId = "test-provider-key" });
      options.ConfigurationManager = new StaticConfigurationManager<OpenIdConnectConfiguration>(metadata);
      options.Backchannel = new HttpClient(new Exchange(this));
    }

    internal async Task<string> Begin(HttpClient client)
    {
      var token = Token(await client.GetStringAsync("/"));
      var response = await client.PostAsync("/auth/login", new FormUrlEncodedContent(new Dictionary<string, string> { ["__RequestVerificationToken"] = token }));
      Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
      var query = QueryHelpers.ParseQuery(response.Headers.Location!.Query);
      Assert.Equal("S256", query["code_challenge_method"]);
      Assert.False(string.IsNullOrEmpty(query["code_challenge"]));
      Assert.Equal("2", query["acr_values"]);
      Assert.Equal("0", query["max_age"]);
      Assert.Equal("code", query["response_type"]);
      nonce = query["nonce"].ToString();
      return QueryHelpers.AddQueryString("/signin-oidc", new Dictionary<string, string?> { ["code"] = "isolated-code", ["state"] = query["state"] });
    }

    private sealed class Exchange(OwnerApplication app) : HttpMessageHandler
    {
      protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
      {
        Assert.Equal("https://identity.invalid/token", request.RequestUri!.AbsoluteUri);
        Assert.Equal(HttpMethod.Post, request.Method);
        var form = QueryHelpers.ParseQuery(await request.Content!.ReadAsStringAsync(cancellationToken));
        if (form["grant_type"] == "refresh_token") throw new HttpRequestException("Isolated provider outage.");
        Assert.Equal("authorization_code", form["grant_type"]);
        Assert.Equal("isolated-code", form["code"]);
        Assert.False(string.IsNullOrEmpty(form["code_verifier"]));
        Interlocked.Increment(ref app.TokenExchanges);
        using var wrong = ECDsa.Create(ECCurve.NamedCurves.nistP256);
        var jwt = new JsonWebTokenHandler().CreateToken(new SecurityTokenDescriptor
        {
          Issuer = app.Invalid == "issuer" ? "https://other.invalid" : "https://identity.invalid/realms/dev",
          Audience = "sofa-development",
          Expires = DateTime.UtcNow.AddMinutes(10),
          Subject = new ClaimsIdentity(new[]
            {
                        new Claim("sub", app.Invalid == "subject" ? "different-owner" : "owner-subject"),
                        new Claim("acr", app.Invalid == "assurance" ? "1" : "2"),
                        new Claim("nonce", app.Invalid == "nonce" ? "wrong-nonce" : app.nonce),
                        new Claim("auth_time", (app.Invalid == "old-ceremony" ? DateTimeOffset.UtcNow.AddMinutes(-6) : DateTimeOffset.UtcNow).ToUnixTimeSeconds().ToString(CultureInfo.InvariantCulture)),
                    }),
          SigningCredentials = new SigningCredentials(new ECDsaSecurityKey(app.Invalid == "signature" ? wrong : app.key) { KeyId = "test-provider-key" }, SecurityAlgorithms.EcdsaSha256),
        });
        return new HttpResponseMessage(HttpStatusCode.OK)
        {
          Content = new StringContent(JsonSerializer.Serialize(new { id_token = jwt, access_token = "access-test-value", refresh_token = "refresh-test-value", token_type = "Bearer", expires_in = 300 }), Encoding.UTF8, "application/json"),
        };
      }
    }

    protected override void Dispose(bool disposing) { base.Dispose(disposing); if (disposing) key.Dispose(); }
  }
}
