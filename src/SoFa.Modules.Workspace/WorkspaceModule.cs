using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using JuanGCarmona.SoFa.Modules.Workspace.Identity;
using JuanGCarmona.SoFa.Modules.Workspace.Source;
using JuanGCarmona.SoFa.Modules.Workspace.Runtime;

namespace JuanGCarmona.SoFa.Modules.Workspace;

public static class WorkspaceModule
{
  public static void ValidateConfiguration(Microsoft.Extensions.Configuration.IConfiguration configuration) => _ = WorkspaceOptions.Load(configuration);

  public static IServiceCollection AddWorkspace(this IServiceCollection services)
  {
    OwnerIdentity.Add(services);
    services.AddSingleton(_ => new GitHubTransport());
    services.AddSingleton<GitHubAuthorization>();
    services.AddSingleton<RepositoryAccess>();
    services.AddSingleton<GetObservationsHandler>();
    services.AddSingleton<GetAccountStatusHandler>();
    services.AddSingleton<GetIssueContextHandler>();
    services.AddSingleton<GetIssueEvidenceHandler>();
    services.AddSingleton<GetFactoryPageHandler>();
    services.AddSingleton<GetRepositoryConfigurationHandler>();
    services.AddSingleton<RuntimeReadSlots>();
    services.AddSingleton<GetHostInventoryHandler>();
    services.AddSingleton<GetRepositoryRunnersHandler>();

    services.AddSingleton<SourceControlBridge>();
    services.AddSingleton<GetIssueControlsHandler>();
    services.AddSingleton<SourceControlAccess>();
    services.AddSingleton<IssueControlCommands>();
    services.AddSingleton(provider =>
    {
      var options = provider.GetRequiredService<WorkspaceOptions>();
      return new ControlJournal(options.ControlJournalDirectory, options.ControlJournalKeyDirectory);
    });
    return services;
  }

  public static WebApplication UseWorkspace(this WebApplication app)
  {
    var settings = app.Services.GetRequiredService<WorkspaceOptions>();
    // Chromium applies form-action to redirects after a same-origin form POST too.
    var authenticationDestinations = settings.Identity.Issuer.GetLeftPart(UriPartial.Authority)
        + (string.IsNullOrEmpty(settings.GitHubClientId) ? "" : " https://github.com");
    app.Use(async (context, next) =>
    {
      var originalCancellation = context.RequestAborted;
      using var budget = CancellationTokenSource.CreateLinkedTokenSource(originalCancellation);
      budget.CancelAfter(TimeSpan.FromSeconds(35));
      context.RequestAborted = budget.Token;
      context.Response.Headers.CacheControl = "no-store";
      context.Response.Headers["Content-Security-Policy"] = "default-src 'none'; style-src 'self'; script-src 'self'; connect-src 'self'; img-src 'self'; form-action 'self' "
              + authenticationDestinations + "; base-uri 'none'; frame-ancestors 'none'";
      context.Response.Headers["X-Content-Type-Options"] = "nosniff";
      context.Response.Headers["Referrer-Policy"] = "no-referrer";
      if (!context.Request.IsHttps || !string.Equals(context.Request.Host.Value, settings.Origin.Authority, StringComparison.OrdinalIgnoreCase))
      { context.Response.StatusCode = 400; return; }
      var documentNavigation = HttpMethods.IsGet(context.Request.Method)
          && context.Request.Headers["Sec-Fetch-Mode"] == "navigate"
          && context.Request.Headers["Sec-Fetch-Dest"] == "document";
      if (context.Request.Headers["Sec-Fetch-Site"] == "cross-site" && !documentNavigation
              && context.Request.Path != "/signin-oidc" && context.Request.Path != "/connections/github/callback")
      { context.Response.StatusCode = 403; return; }
      try { await next(context).ConfigureAwait(false); }
      catch (OperationCanceledException) when (originalCancellation.IsCancellationRequested) { }
      catch (Exception) when (!context.Response.HasStarted)
      {
        context.Response.Clear();
        context.Response.StatusCode = 503;
        context.Response.Headers.CacheControl = "no-store";
        await context.Response.WriteAsync("The workspace is temporarily unavailable.").ConfigureAwait(false);
      }
      finally { context.RequestAborted = originalCancellation; }
    });
    app.UseRouting();
    app.UseAuthentication();
    app.Use(OwnerIdentity.Gate);
    app.UseAuthorization();
    app.MapGet("/", async (HttpContext context, IAntiforgery antiforgery, OwnerSessions sessions, ProviderSessions provider) =>
    {
      if (context.User.Identity?.IsAuthenticated == true && sessions.Get(context.User) is { } current
          && await provider.EnsureCurrentAsync(current, context.RequestAborted).ConfigureAwait(false) == ProviderAuthority.Current)
      { context.Response.Redirect("/workspace/operations"); return Results.Empty; }
      var token = antiforgery.GetAndStoreTokens(context).RequestToken!;
      return Html("Lighthouse Control Center", "<p>Sign in with your configured identity provider to open the private workspace.</p>"
              + (context.Request.Query["access"] == "denied" ? "<p role=\"alert\">Sign-in was not accepted. Use the configured owner account and complete the required sign-in.</p>" : "")
              + Form("/auth/login", token, "Sign in"));
    }).AllowAnonymous();
    app.MapGet("/health/live", () => Results.Text("live")).AllowAnonymous();
    foreach (var asset in new[] { (Path: "/workspace-ui.js", File: "workspace-ui.js", Type: "text/javascript"),
        (Path: "/read-projection-cache.js", File: "read-projection-cache.js", Type: "text/javascript"),
        (Path: "/lighthouse.svg", File: "lighthouse.svg", Type: "image/svg+xml") })
      app.MapGet(asset.Path, async (CancellationToken cancellationToken) => Results.Content(
          await File.ReadAllTextAsync(Path.Combine(AppContext.BaseDirectory, "workspace-assets", asset.File), cancellationToken).ConfigureAwait(false), asset.Type)).AllowAnonymous();
    app.MapPost("/auth/login", async (HttpContext context, IAntiforgery antiforgery) =>
    {
      if (!await Csrf(context, antiforgery).ConfigureAwait(false)) return;
      await context.ChallengeAsync(IdentitySchemes.Oidc, new AuthenticationProperties { RedirectUri = "/workspace" }).ConfigureAwait(false);
    }).AllowAnonymous();
    app.MapPost("/auth/logout", async (HttpContext context, IAntiforgery antiforgery, OwnerSessions sessions) =>
    {
      if (!await Csrf(context, antiforgery).ConfigureAwait(false)) return;
      var record = sessions.GetAuthenticatedRequest(context);
      if (record is not null) sessions.Remove(record);
      await context.SignOutAsync(IdentitySchemes.Cookie).ConfigureAwait(false);
      context.Response.Redirect("/");
    }).WithMetadata(new OwnerIdentity.LocalSignOut());
    app.MapGet("/workspace", async (HttpContext context, IAntiforgery antiforgery, OwnerSessions sessions, GitHubAuthorization github, TimeProvider clock) =>
    {
      var record = sessions.Get(context.User)!;
      sessions.TouchUserActivity(record.Id);
      var token = antiforgery.GetAndStoreTokens(context).RequestToken!;
      var connection = await github.CurrentAsync(record, context.RequestAborted).ConfigureAwait(false);
      var feedback = context.Request.Query["connection"].ToString() switch
      {
        "connected" => "GitHub account connected. Current repository authority is shown below.",
        "confirmationrequired" => "Review the verified replacement account and confirm it explicitly below.",
        "rejected" => "The connection request was not accepted. Start a new connection if needed.",
        "unavailable" => "GitHub authorization could not be verified. Try connecting again.",
        _ => "",
      };
      string account;
      bool canDisconnect;
      lock (record.Sync)
      {
        canDisconnect = record.GitHub.Current is not null;
        account = connection is not null ? "<p id=\"github-account\">GitHub acting account: <a href=\"https://github.com/"
                + Encode(Uri.EscapeDataString(connection.Login)) + "\" rel=\"noreferrer noopener\">" + Encode(connection.Login) + "</a>.</p>"
                + "<details><summary>Technical identity details</summary><p>GitHub user ID "
                + connection.UserId.ToString(System.Globalization.CultureInfo.InvariantCulture) + ". Verified at "
                + Encode(connection.VerifiedAt.ToString("o", System.Globalization.CultureInfo.InvariantCulture)) + ". Identity provider: "
                + Encode(settings.Identity.Issuer.AbsoluteUri) + ".</p></details>"
                : "<p id=\"github-account\">" + (record.GitHub.Current is null ? "GitHub is not connected." : "GitHub account verification is unavailable. Source operations are blocked.") + "</p>";
        if (record.GitHub.Replacement is { } candidate && clock.GetUtcNow() < record.GitHub.ReplacementDeadline)
          account += "<p>Confirm replacing the acting account with " + Encode(candidate.Login) + ".</p>"
                  + "<details><summary>Replacement identity details</summary><p>GitHub user ID "
                  + candidate.UserId.ToString(System.Globalization.CultureInfo.InvariantCulture) + ".</p></details>"
                  + Form("/connections/github/confirm", token, "Confirm account replacement");
      }
      return Html("Workspace", "<p>Owner identity verified. GitHub account connection is separate.</p>"
          + (feedback.Length == 0 ? "" : "<p role=\"status\">" + feedback + "</p>")
              + account
              + "<p id=\"account-status\" role=\"status\" aria-live=\"polite\">Checking current repository authority…</p><ul id=\"repository-authority\" aria-label=\"Configured repository permissions\"></ul>"
              + (string.IsNullOrEmpty(settings.GitHubClientId) ? "<h2>GitHub setup required</h2><p>Register the development GitHub App, then connect your GitHub account to load repository operations.</p>"
                  : Form("/connections/github/start", token, "Connect GitHub account"))
              + "<div id=\"github-disconnect\"" + (canDisconnect ? "" : " hidden") + ">"
              + Form("/connections/github/disconnect", token, "Disconnect GitHub") + "</div>"
              + "<p><a href=\"/workspace/operations\">Open fleet operations</a></p>"
              + "<p>Disconnect removes the locally stored GitHub grant for this owner across active sessions. To revoke the provider-side grant too, use <a href=\"https://github.com/settings/apps/authorizations\" rel=\"noreferrer\">GitHub authorized Apps</a>.</p>"
              + "<p>Owner sessions expire after two hours without user activity or twelve hours after authentication. Identity-provider authority is checked online at least every five minutes; a failed due check blocks access.</p>"
              + "<p>Sign out ends this browser's owner session. The separately authorized GitHub connection remains available for a later owner session and is checked with GitHub before use.</p>"
              + Form("/auth/logout", token, "Sign out"), account: true);
    });
    app.MapPost("/connections/github/start", async (HttpContext context, IAntiforgery antiforgery, OwnerSessions sessions, GitHubAuthorization github) =>
    {
      if (!await Csrf(context, antiforgery).ConfigureAwait(false)) return;
      var record = sessions.Get(context.User)!;
      var redirect = github.Begin(record);
      if (redirect is null) { context.Response.StatusCode = 503; return; }
      sessions.TouchUserActivity(record.Id);
      context.Response.Redirect(redirect);
    });
    app.MapGet("/connections/github/callback", async (HttpContext context, OwnerSessions sessions, GitHubAuthorization github) =>
    {
      var query = context.Request.Query;
      if (query["state"].Count != 1 || query["code"].Count > 1 || query["error"].Count > 1)
      { context.Response.StatusCode = 400; return; }
      var record = sessions.Get(context.User)!;
      var outcome = await github.CompleteAsync(record, query["state"].ToString(),
              query["error"].Count == 0 ? query["code"].ToString() : null, context.RequestAborted).ConfigureAwait(false);
      if (outcome == ConnectionOutcome.Connected) await sessions.RotateCookieAsync(context, record).ConfigureAwait(false);
      context.Response.Redirect("/workspace?connection=" + outcome.ToString().ToLowerInvariant());
    });
    app.MapPost("/connections/github/confirm", async (HttpContext context, IAntiforgery antiforgery, OwnerSessions sessions, GitHubAuthorization github) =>
    {
      if (!await Csrf(context, antiforgery).ConfigureAwait(false)) return;
      var record = sessions.Get(context.User)!;
      if (!github.Confirm(record)) { context.Response.StatusCode = 409; return; }
      await sessions.RotateCookieAsync(context, record).ConfigureAwait(false);
      sessions.TouchUserActivity(record.Id);
      context.Response.Redirect("/workspace");
    });
    app.MapPost("/connections/github/disconnect", async (HttpContext context, IAntiforgery antiforgery, OwnerSessions sessions) =>
    {
      if (!await Csrf(context, antiforgery).ConfigureAwait(false)) return;
      var record = sessions.GetAuthenticatedRequest(context);
      if (record is null) { await OwnerIdentity.AccessFailure(context, 401).ConfigureAwait(false); return; }
      GitHubAuthorization.Disconnect(record);
      await sessions.RotateCookieAsync(context, record).ConfigureAwait(false);
      sessions.TouchUserActivity(record.Id);
      context.Response.Redirect("/workspace");
    }).WithMetadata(new OwnerIdentity.LocalSignOut());
    app.MapGet("/api/session", (HttpContext context, OwnerSessions sessions) =>
    {
      var record = sessions.Get(context.User)!;
      return Results.Json(new
      {
        authenticated = true,
        displayName = context.User.FindFirst("preferred_username")?.Value ?? context.User.Identity?.Name ?? "Signed in",
        idleDeadline = record.IdleDeadline,
        absoluteDeadline = record.AbsoluteDeadline,
        providerValidatedAt = record.ProviderValidatedAt,
        dataScope = record.GitHub.Current?.DataScope
      });
    });
    app.MapGet("/api/observations", async (HttpContext context, OwnerSessions sessions, GetObservationsHandler observations) =>
    {
      var record = sessions.Get(context.User)!;
      var snapshot = await observations.Handle(new GetObservations(record), context.RequestAborted).ConfigureAwait(false);
      if (snapshot is not null) return Results.Json(snapshot);
      lock (record.Sync)
        return Results.Json(new
        {
          state = string.IsNullOrEmpty(settings.GitHubClientId) ? "unconfigured"
            : record.GitHub.Current is null ? "unlinked" : "unavailable"
        }, statusCode: 409);
    });
    app.MapGet("/api/connections/github", async (HttpContext context, OwnerSessions sessions, GetAccountStatusHandler status) =>
        Results.Json(await status.Handle(new GetAccountStatus(sessions.Get(context.User)!), context.RequestAborted).ConfigureAwait(false)));
    app.MapGet("/api/factory/{owner}/{repository}", async (string owner, string repository,
        HttpContext context, OwnerSessions sessions, GetFactoryPageHandler factory) =>
    {
      var page = context.Request.Query["page"].Count == 0 ? 1
          : context.Request.Query["page"].Count == 1 && int.TryParse(context.Request.Query["page"], out var selected) ? selected : 0;
      int? number = context.Request.Query["number"].Count == 0 ? null
          : context.Request.Query["number"].Count == 1 && int.TryParse(context.Request.Query["number"], out var exact) ? exact : 0;
      var result = await factory.Handle(new(sessions.Get(context.User)!, owner + "/" + repository, page, number), context.RequestAborted).ConfigureAwait(false);
      if (System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(result).Length > 2 * 1024 * 1024)
        result = result with { Availability = "unavailable", Issues = [], Complete = false, NextPage = null, Reason = "The projected page exceeded the2MiB response bound; coverage is unknown." };
      return Results.Json(result, statusCode: result.Availability switch
      { "invalid" => 400, "denied" => 403, "disconnected" => 409, "unavailable" => 503, _ => 200 });
    });
    app.MapGet("/api/repositories/{owner}/{repository}/configuration", async (string owner, string repository,
        HttpContext context, OwnerSessions sessions, GetRepositoryConfigurationHandler configuration) =>
    {
      var result = await configuration.Handle(new(sessions.Get(context.User)!, owner + "/" + repository), context.RequestAborted).ConfigureAwait(false);
      var payload = System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(result);
      if (payload.Length > 2 * 1024 * 1024)
        return Results.Json(result with
        {
          Availability = "unavailable",
          Coverage = "unknown",
          Sources = [],
          Facts = [],
          Reasons = ["The projected configuration exceeded the 2 MiB response limit; no partial facts were returned."]
        }, statusCode: 503);
      return Results.Json(result, statusCode: result.Availability switch
      { "invalid" => 400, "denied" => 403, "disconnected" => 409, "unavailable" => 503, _ => 200 });
    });
    app.MapGet("/workspace/attention", async (HttpContext context, OwnerSessions sessions) =>
    {
      sessions.TouchUserActivity(sessions.Get(context.User)!.Id);
      return Results.Content(await File.ReadAllTextAsync(Path.Combine(AppContext.BaseDirectory, "workspace-assets", "attention.html"), context.RequestAborted).ConfigureAwait(false), "text/html");
    });
    foreach (var route in new[] { "/workspace/factory", "/workspace/repositories", "/workspace/repositories/{owner}/{repository}" })
    {
      app.MapGet(route, async (HttpContext context, OwnerSessions sessions) =>
      {
        sessions.TouchUserActivity(sessions.Get(context.User)!.Id);
        if (context.Request.RouteValues["owner"] is string owner && context.Request.RouteValues["repository"] is string repository
            && !settings.Repositories.Contains(owner + "/" + repository, StringComparer.OrdinalIgnoreCase)) return (IResult)Results.NotFound();
        return Results.Content(await File.ReadAllTextAsync(Path.Combine(AppContext.BaseDirectory, "workspace-assets", "factory.html"), context.RequestAborted).ConfigureAwait(false), "text/html");
      });
    }
    app.MapGet("/api/issues/{owner}/{repository}/{number:int}", async (string owner, string repository, int number,
        HttpContext context, OwnerSessions sessions, GetIssueContextHandler issues) =>
    {
      var page = context.Request.Query["page"].Count == 0 ? 1
          : context.Request.Query["page"].Count == 1 && int.TryParse(context.Request.Query["page"], out var selected) ? selected : 0;
      var result = await issues.Handle(new GetIssueContext(sessions.Get(context.User)!, owner + "/" + repository,
          number, page, context.Request.Query["snapshot"]), context.RequestAborted).ConfigureAwait(false);
      if (System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(result).Length > 2 * 1024 * 1024)
        result = result with
        {
          Conversation = new("unavailable", [], page, 100, result.Conversation?.SourceCount, null, false,
          "Conversation projection exceeds the 2 MiB response bound; no empty collection is inferred.")
        };
      if (System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(result).Length > 2 * 1024 * 1024) return Results.StatusCode(503);
      return Results.Json(result, statusCode: result.Availability switch
      { "denied" => 403, "disconnected" => 409, "unavailable" => 503, "invalid" => 400, "changed" => 409, _ => 200 });
    });
    app.MapGet("/api/issues/{owner}/{repository}/{number:int}/evidence", async (string owner, string repository, int number,
        HttpContext context, OwnerSessions sessions, GetIssueEvidenceHandler evidence) =>
    {
      var page = context.Request.Query["page"].Count == 0 ? 1
          : context.Request.Query["page"].Count == 1 && int.TryParse(context.Request.Query["page"], out var selected) ? selected : 0;
      var result = await evidence.Handle(new(new(sessions.Get(context.User)!, owner + "/" + repository,
          number, page, context.Request.Query["snapshot"])), context.RequestAborted).ConfigureAwait(false);
      if (System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(result).Length > 2 * 1024 * 1024)
        result = new("available", result.ObservedAt, [], result.Scope,
          "Evidence projection exceeds the 2 MiB response bound; source reports remain readable and this is not evidence of absent work.");
      return Results.Json(result, statusCode: result.Availability switch
      { "denied" => 403, "disconnected" => 409, "unavailable" => 503, "changed" => 409, _ => 200 });
    });
    app.MapGet("/api/issues/{owner}/{repository}/{number:int}/controls", async (string owner, string repository, int number,
        HttpContext context, OwnerSessions sessions, GetIssueControlsHandler controls) =>
    {
      var result = await controls.Handle(new(sessions.Get(context.User)!, owner + "/" + repository, number), context.RequestAborted).ConfigureAwait(false);
      return Results.Json(result, statusCode: result.Availability switch
      { "denied" => 403, "disconnected" => 409, "unavailable" => 503, "invalid" => 400, _ => 200 });
    });
    app.MapGet("/api/controls/csrf", (HttpContext context, IAntiforgery antiforgery) =>
        Results.Json(new { token = antiforgery.GetAndStoreTokens(context).RequestToken }));
    app.MapPost("/api/issues/{owner}/{repository}/{number:int}/controls", async (string owner, string repository, int number,
        HttpContext context, IAntiforgery antiforgery, OwnerSessions sessions, IssueControlCommands commands) =>
    {
      if (!await Csrf(context, antiforgery).ConfigureAwait(false)) return Results.StatusCode(400);
      if (context.Request.ContentLength is > 16384 || !context.Request.HasJsonContentType()) return Results.StatusCode(400);
      using var input = new MemoryStream();
      var bytes = new byte[4096]; int read;
      while ((read = await context.Request.Body.ReadAsync(bytes, context.RequestAborted).ConfigureAwait(false)) > 0)
      {
        if (input.Length + read > 16384) return Results.StatusCode(400);
        input.Write(bytes, 0, read);
      }
      ControlCommand? command;
      try
      {
        using var value = System.Text.Json.JsonDocument.Parse(input.ToArray());
        if (value.RootElement.ValueKind != System.Text.Json.JsonValueKind.Object
            || value.RootElement.EnumerateObject().Select(property => property.Name).Distinct(StringComparer.Ordinal).Count() != value.RootElement.EnumerateObject().Count()
            || value.RootElement.EnumerateObject().Any(property => property.Name is not ("operation" or "action" or "body" or "id"))) return Results.StatusCode(400);
        command = System.Text.Json.JsonSerializer.Deserialize<ControlCommand>(input.ToArray(), System.Text.Json.JsonSerializerOptions.Web);
      }
      catch (System.Text.Json.JsonException) { return Results.StatusCode(400); }
      if (command is null) return Results.StatusCode(400);
      var record = sessions.Get(context.User)!;
      sessions.TouchUserActivity(record.Id);
      var result = await commands.Handle(new(record, owner + "/" + repository, number), command, context.RequestAborted).ConfigureAwait(false);
      return Results.Json(result, statusCode: result.Availability switch
      { "denied" => 403, "disconnected" or "conflict" => 409, "unavailable" => 503, "invalid" => 400, _ => 200 });
    });
    app.MapGet("/workspace/issues", async (HttpContext context, OwnerSessions sessions) =>
    {
      sessions.TouchUserActivity(sessions.Get(context.User)!.Id);
      return Results.Content(await File.ReadAllTextAsync(Path.Combine(AppContext.BaseDirectory, "workspace-assets", "factory.html"), context.RequestAborted).ConfigureAwait(false), "text/html");
    });
    app.MapGet("/workspace/issues/{owner}/{repository}/{number:int}", async (HttpContext context, OwnerSessions sessions) =>
    {
      sessions.TouchUserActivity(sessions.Get(context.User)!.Id);
      return Results.Content(await File.ReadAllTextAsync(Path.Combine(AppContext.BaseDirectory, "workspace-assets", "issue.html"), context.RequestAborted).ConfigureAwait(false), "text/html");
    });
    app.MapGet("/workspace/operations", async (HttpContext context, OwnerSessions sessions) =>
    {
      sessions.TouchUserActivity(sessions.Get(context.User)!.Id);
      return Results.Content(await File.ReadAllTextAsync(Path.Combine(AppContext.BaseDirectory, "workspace-assets", "index.html"), context.RequestAborted).ConfigureAwait(false), "text/html");
    });
    foreach (var route in new[] { "/workspace/health", "/workspace/health/{owner}/{repository}", "/workspace/health/{owner}/{repository}/runs/{run:long}" })
    {
      app.MapGet(route, async (HttpContext context, OwnerSessions sessions) =>
      {
        sessions.TouchUserActivity(sessions.Get(context.User)!.Id);
        if (context.Request.RouteValues["owner"] is string owner && context.Request.RouteValues["repository"] is string repository
            && !settings.Repositories.Contains(owner + "/" + repository, StringComparer.OrdinalIgnoreCase)) return (IResult)Results.NotFound();
        return Results.Content(await File.ReadAllTextAsync(Path.Combine(AppContext.BaseDirectory, "workspace-assets", "health.html"), context.RequestAborted).ConfigureAwait(false), "text/html");
      });
    }
    app.MapGet("/workspace/runtime", async (HttpContext context, OwnerSessions sessions) =>
    {
      sessions.TouchUserActivity(sessions.Get(context.User)!.Id);
      return Results.Content(await File.ReadAllTextAsync(Path.Combine(AppContext.BaseDirectory, "workspace-assets", "runtime.html"), context.RequestAborted).ConfigureAwait(false), "text/html");
    });
    app.MapGet("/api/runtime/host", async (HttpContext context, OwnerSessions sessions, GetHostInventoryHandler host) =>
    {
      var record = sessions.Get(context.User)!;
      var result = await host.Handle(new(record), context.RequestAborted).ConfigureAwait(false);
      if (!ReferenceEquals(sessions.Get(record.Id), record)) return Results.StatusCode(401);
      result = GetHostInventoryHandler.Bound(result);
      return Results.Json(result, statusCode: result.Availability switch { "disconnected" => 401, "busy" => 429, "unavailable" => 503, _ => 200 });
    });
    app.MapGet("/api/runtime/{owner}/{repository}", async (string owner, string repository,
        HttpContext context, OwnerSessions sessions, GetRepositoryRunnersHandler runners) =>
    {
      var record = sessions.Get(context.User)!;
      var result = await runners.Handle(new(record, owner + "/" + repository), context.RequestAborted).ConfigureAwait(false);
      if (!ReferenceEquals(sessions.Get(record.Id), record)) return Results.StatusCode(401);
      result = GetRepositoryRunnersHandler.Bound(result);
      return Results.Json(result, statusCode: result.Availability switch { "disconnected" => 409, "denied" => 403, "busy" => 429, "unavailable" => 503, _ => 200 });
    });
    foreach (var asset in new[] { (Path: "/app.js", File: "app.js", Type: "text/javascript"),
            (Path: "/app.css", File: "app.css", Type: "text/css"), (Path: "/tokens.css", File: "tokens.css", Type: "text/css"),
            (Path: "/factory.js", File: "factory.js", Type: "text/javascript"),
            (Path: "/attention.js", File: "attention.js", Type: "text/javascript"),
            (Path: "/attention-controls.js", File: "attention-controls.js", Type: "text/javascript"),
            (Path: "/factory.css", File: "factory.css", Type: "text/css"),
            (Path: "/runtime.js", File: "runtime.js", Type: "text/javascript"), (Path: "/runtime.css", File: "runtime.css", Type: "text/css"),
            (Path: "/health.js", File: "health.js", Type: "text/javascript"), (Path: "/health.css", File: "health.css", Type: "text/css"),
            (Path: "/issue.js", File: "issue.js", Type: "text/javascript"),
            (Path: "/issue-actions.js", File: "issue-actions.js", Type: "text/javascript"),
            (Path: "/markdown.js", File: "markdown.js", Type: "text/javascript"), (Path: "/issue.css", File: "issue.css", Type: "text/css"),
            (Path: "/account.css", File: "account.css", Type: "text/css"), (Path: "/account.js", File: "account.js", Type: "text/javascript") })
      app.MapGet(asset.Path, async (HttpContext context, CancellationToken cancellationToken) => Results.Content(
          await File.ReadAllTextAsync(Path.Combine(AppContext.BaseDirectory, "workspace-assets", asset.File), cancellationToken).ConfigureAwait(false), asset.Type)).AllowAnonymous();
    return app;
  }

  private static IResult Html(string title, string body, bool account = false) => Results.Content(
      "<!doctype html><html lang=\"en\" data-lh-theme=\"dark\"><head><meta charset=\"utf-8\"><meta name=\"viewport\" content=\"width=device-width,initial-scale=1\"><title>Lighthouse Control Center · "
      + title + "</title><link rel=\"icon\" href=\"/lighthouse.svg\" type=\"image/svg+xml\"><link rel=\"stylesheet\" href=\"/tokens.css\"><link rel=\"stylesheet\" href=\"/app.css\"><link rel=\"stylesheet\" href=\"/account.css\">"
      + "<script type=\"module\" src=\"/workspace-ui.js\"></script>" + (account ? "<script type=\"module\" src=\"/account.js\"></script>" : "")
      + "</head><body><a class=\"skip\" href=\"#account\">Skip to account</a><header class=\"topbar\"><a class=\"brand\" href=\"/\">Lighthouse Control Center</a><nav aria-label=\"Workspace navigation\"><a href=\"/workspace/attention\">Attention</a> <a href=\"/workspace\">Account</a> <a href=\"/workspace/operations\">Operations</a> <a href=\"/workspace/repositories\">Repositories</a> <a href=\"/workspace/issues\">Issues</a> <a href=\"/workspace/runtime\">Runtime</a></nav></header><main id=\"account\" class=\"account-panel\" tabindex=\"-1\"><h1>" + title + "</h1>" + body + "</main></body></html>", "text/html");

  private static string Form(string action, string token, string label) => "<form method=\"post\" action=\"" + action
      + "\"><input type=\"hidden\" name=\"__RequestVerificationToken\" value=\"" + HtmlEncoder.Default.Encode(token)
      + "\"><button type=\"submit\">" + label + "</button></form>";

  private static string Encode(string value) => HtmlEncoder.Default.Encode(value);

  private static async Task<bool> Csrf(HttpContext context, IAntiforgery antiforgery)
  {
    try { await antiforgery.ValidateRequestAsync(context).ConfigureAwait(false); return true; }
    catch (AntiforgeryValidationException) { context.Response.StatusCode = 400; return false; }
  }
}
