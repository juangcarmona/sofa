using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;

namespace JuanGCarmona.SoFa.Modules.Workspace.Identity;

internal static class OwnerIdentity
{
  internal sealed record LocalSignOut;

  internal static void Add(IServiceCollection services)
  {
    services.AddDataProtection().UseEphemeralDataProtectionProvider();
    services.AddSingleton(sp => WorkspaceOptions.Load(sp.GetRequiredService<IConfiguration>()));
    services.AddSingleton(sp => sp.GetRequiredService<WorkspaceOptions>().Identity);
    services.AddSingleton(TimeProvider.System);
    services.AddSingleton<OwnerSessions>();
    services.AddSingleton<ProviderSessions>();
    services.AddAuthentication(options =>
    {
      options.DefaultScheme = IdentitySchemes.Cookie;
      // Login is an explicit antiforgery-protected POST; navigation never creates a challenge.
      options.DefaultChallengeScheme = IdentitySchemes.Cookie;
    }).AddCookie(IdentitySchemes.Cookie, options =>
    {
      options.Cookie.Name = "__Host-SoFa.Owner";
      options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
      options.Cookie.HttpOnly = true;
      options.Cookie.SameSite = SameSiteMode.Lax;
      options.Cookie.Path = "/";
      options.SlidingExpiration = false;
      options.Events = new CookieAuthenticationEvents
      {
        OnRedirectToLogin = context => AccessFailure(context.HttpContext, 401),
        OnRedirectToAccessDenied = context => AccessFailure(context.HttpContext, 403),
        OnValidatePrincipal = context =>
            {
              var record = context.HttpContext.RequestServices.GetRequiredService<OwnerSessions>().Get(context.Principal!);
              if (record is null) context.RejectPrincipal();
              else context.HttpContext.Items[typeof(OwnerSession)] = record;
              return Task.CompletedTask;
            },
      };
    }).AddCookie(IdentitySchemes.Issuance, options =>
    {
      // A separate issuance handler cannot renew the request's old ticket reference.
      // Cookie serialization, protection and response handling remain framework-owned.
      options.Cookie.Name = "__Host-SoFa.Owner";
      options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
      options.Cookie.HttpOnly = true;
      options.Cookie.SameSite = SameSiteMode.Lax;
      options.Cookie.Path = "/";
      options.SlidingExpiration = false;
      options.CookieManager = new IssuanceCookieManager();
    }).AddOpenIdConnect(IdentitySchemes.Oidc, _ => { });
    services.AddOptions<CookieAuthenticationOptions>(IdentitySchemes.Cookie)
        .Configure<OwnerSessions, TimeProvider>((options, sessions, clock) =>
        { options.SessionStore = sessions; options.TimeProvider = clock; });
    services.AddOptions<CookieAuthenticationOptions>(IdentitySchemes.Issuance)
        .Configure<OwnerSessions, TimeProvider, IServiceProvider>((options, sessions, clock, provider) =>
        {
          options.SessionStore = sessions;
          options.TimeProvider = clock;
          options.TicketDataFormat = provider.GetRequiredService<Microsoft.Extensions.Options.IOptionsMonitor<CookieAuthenticationOptions>>()
                  .Get(IdentitySchemes.Cookie).TicketDataFormat;
        });
    services.AddOptions<OpenIdConnectOptions>(IdentitySchemes.Oidc)
        .Configure<IdentityProfile, TimeProvider>((options, profile, clock) =>
        {
          options.TimeProvider = clock;
          options.Authority = profile.Issuer.AbsoluteUri;
          options.SignInScheme = IdentitySchemes.Issuance;
          options.ClientId = profile.ClientId;
          options.ClientSecret = profile.ClientSecret;
          options.ResponseType = OpenIdConnectResponseType.Code;
          options.ResponseMode = OpenIdConnectResponseMode.Query;
          options.UsePkce = true;
          options.RequireHttpsMetadata = true;
          options.MapInboundClaims = false;
          options.ClaimActions.Clear();
          options.SaveTokens = true; // The cookie contains only the server ticket reference.
          options.Scope.Clear();
          options.Scope.Add("openid");
          options.MaxAge = TimeSpan.Zero;
          options.RemoteAuthenticationTimeout = TimeSpan.FromMinutes(5);
          options.ProtocolValidator.NonceLifetime = TimeSpan.FromMinutes(5);
          options.CallbackPath = "/signin-oidc";
          options.RemoteSignOutPath = PathString.Empty;
          options.TokenValidationParameters.ValidIssuer = profile.Issuer.AbsoluteUri;
          options.TokenValidationParameters.ValidAudience = profile.ClientId;
          options.TokenValidationParameters.ClockSkew = TimeSpan.FromSeconds(60);
          options.BackchannelHttpHandler = new HttpClientHandler { AllowAutoRedirect = false, UseCookies = false };
          options.BackchannelTimeout = TimeSpan.FromSeconds(10);
          options.Events = new OpenIdConnectEvents
          {
            OnRedirectToIdentityProvider = context =>
                {
                  context.ProtocolMessage.AcrValues = "2";
                  return Task.CompletedTask;
                },
            OnTokenValidated = context =>
                {
                  var now = context.HttpContext.RequestServices.GetRequiredService<TimeProvider>().GetUtcNow();
                  if (context.Principal is null || OwnerAdmission.Validate(context.Principal, profile, now, true) is null
                          || string.IsNullOrEmpty(context.TokenEndpointResponse?.AccessToken)
                          || string.IsNullOrEmpty(context.TokenEndpointResponse?.RefreshToken))
                    context.Fail("The identity cannot establish an owner session.");
                  return Task.CompletedTask;
                },
            OnRemoteFailure = context =>
                {
                  context.HandleResponse();
                  context.Response.Redirect("/?access=denied");
                  return Task.CompletedTask;
                },
            OnTicketReceived = async context =>
                {
                  var previous = await context.HttpContext.AuthenticateAsync(IdentitySchemes.Cookie).ConfigureAwait(false);
                  var sessions = context.HttpContext.RequestServices.GetRequiredService<OwnerSessions>();
                  if (previous.Principal is not null && sessions.Get(previous.Principal) is { } record)
                    sessions.Remove(record);
                },
          };
        });
    services.AddAuthorizationBuilder().SetFallbackPolicy(new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build());
    services.AddAntiforgery(options =>
    {
      options.Cookie.Name = "__Host-SoFa.Csrf";
      options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
      options.Cookie.HttpOnly = true;
      options.Cookie.SameSite = SameSiteMode.Strict;
      options.HeaderName = "X-SoFa-Csrf";
    });
  }

  internal static async Task Gate(HttpContext context, RequestDelegate next)
  {
    if (context.GetEndpoint()?.Metadata.GetMetadata<IAllowAnonymous>() is null && context.User.Identity?.IsAuthenticated == true)
    {
      var sessions = context.RequestServices.GetRequiredService<OwnerSessions>();
      var localRemoval = context.GetEndpoint()?.Metadata.GetMetadata<LocalSignOut>() is not null;
      var record = localRemoval ? sessions.GetAuthenticatedRequest(context) : sessions.Get(context.User);
      if (record is null) { await AccessFailure(context, 401).ConfigureAwait(false); return; }
      // Removing local authority must remain possible during an identity-provider outage.
      if (localRemoval)
      { await next(context).ConfigureAwait(false); return; }
      var authority = await context.RequestServices.GetRequiredService<ProviderSessions>()
          .EnsureCurrentAsync(record, context.RequestAborted).ConfigureAwait(false);
      if (authority != ProviderAuthority.Current)
      {
        await AccessFailure(context, authority == ProviderAuthority.Revoked ? 401 : 503).ConfigureAwait(false);
        return;
      }
    }
    await next(context).ConfigureAwait(false);
  }

  internal static Task AccessFailure(HttpContext context, int status)
  {
    context.Response.StatusCode = status;
    context.Response.Headers.CacheControl = "no-store";
    if (context.Request.Path.StartsWithSegments("/api")) return Task.CompletedTask;
    var title = status == 401 ? "Sign in required" : status == 403 ? "Access denied" : "Verification unavailable";
    context.Response.ContentType = "text/html; charset=utf-8";
    return context.Response.WriteAsync("<!doctype html><html lang=\"en\"><head><meta charset=\"utf-8\"><meta name=\"viewport\" content=\"width=device-width,initial-scale=1\"><title>SoFa · "
      + title + "</title><link rel=\"stylesheet\" href=\"/tokens.css\"><link rel=\"stylesheet\" href=\"/app.css\"><link rel=\"stylesheet\" href=\"/account.css\"></head><body><main class=\"account-panel\"><h1>"
      + title + "</h1><p>Protected workspace data requires a current owner session and successful identity verification.</p><a href=\"/\">Open sign-in and local session controls</a></main></body></html>");
  }

  private sealed class IssuanceCookieManager : ICookieManager
  {
    private readonly ChunkingCookieManager cookies = new();
    public string? GetRequestCookie(HttpContext context, string key) => null;
    public void AppendResponseCookie(HttpContext context, string key, string? value, CookieOptions options) =>
        cookies.AppendResponseCookie(context, key, value, options);
    public void DeleteCookie(HttpContext context, string key, CookieOptions options) => cookies.DeleteCookie(context, key, options);
  }
}
