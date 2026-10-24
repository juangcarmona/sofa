using Microsoft.AspNetCore.Connections.Features;
using Npgsql;
using SoFa.Runtime;
using System.Net.Sockets;
using System.Text.Json;
using System.Text.Json.Nodes;

if (!OperatingSystem.IsLinux()) throw new PlatformNotSupportedException("SoFa's local bridge requires Linux peer credentials");
var builder = WebApplication.CreateBuilder(args);
var options = builder.Configuration.GetSection("Runtime").Get<RuntimeOptions>() ?? throw new InvalidOperationException("Runtime configuration is required");
Directory.CreateDirectory(Path.GetDirectoryName(options.Socket)!);
Directory.CreateDirectory(options.WorkRoot);
// A socket left by a stopped process is removed by the operator, never by a second supervisor.
builder.WebHost.ConfigureKestrel(server =>
{
    server.Limits.MaxRequestBodySize = 32768;
    server.ListenUnixSocket(options.Socket, listen => listen.Use(next => async connection =>
    {
        var socket = connection.Features.Get<IConnectionSocketFeature>()?.Socket;
        if (socket is null || !PeerIdentity.IsAuthorized(socket, options.ClientUid)) { connection.Abort(); return; }
        await next(connection);
    }));
});
builder.Services.AddSingleton(options);
builder.Services.AddSingleton(NpgsqlDataSource.Create(builder.Configuration.GetConnectionString("sofa") ?? throw new InvalidOperationException("ConnectionStrings:sofa is required")));
builder.Services.AddSingleton<RunStore>(); builder.Services.AddSingleton<Lighthouse>(); builder.Services.AddSingleton<Engine>();
builder.Services.AddHttpClient<Github>(client => client.Timeout = TimeSpan.FromSeconds(30));
builder.Services.AddSingleton<Supervisor>(); builder.Services.AddHostedService(services => services.GetRequiredService<Supervisor>());
var app = builder.Build();
app.Use(async (context, next) =>
{
    try { await next(context); }
    catch (Rejected error) { context.Response.StatusCode = 409; await context.Response.WriteAsJsonAsync(new { error = error.Message }); }
    catch (JsonException) { context.Response.StatusCode = 400; await context.Response.WriteAsJsonAsync(new { error = "Invalid JSON" }); }
});
app.MapGet("/health", (Supervisor supervisor) => Results.Json(new { ready = supervisor.Ready }, statusCode: supervisor.Ready ? 200 : 503));
app.MapGet("/policy", async (Lighthouse lighthouse, CancellationToken token) =>
{
    var policy = await lighthouse.Policy(token);
    policy["policyDigest"] = Lighthouse.Hash(await File.ReadAllTextAsync(options.PolicyFile, token));
    return Results.Json(policy);
});
app.MapPost("/route", async (JsonObject request, RunStore store, Lighthouse lighthouse, Github github, CancellationToken token) =>
{
    if (await store.ForWorkflow(request["workflowRunId"]!.GetValue<long>(), request["issue"]!.GetValue<long>(), token) is { } prior && JsonNode.Parse(prior.Request)?["authorization"] is JsonObject authorization)
        return Results.Json(new JsonObject { ["action"] = "run", ["stage"] = "refine", ["route"] = "supervised", ["authorization"] = authorization.DeepClone() });
    var issue = await github.CheckSource(request, token);
    var label = await github.LabelEvent(request, token);
    var policy = await lighthouse.Policy(token);
    var facts = new JsonObject
    {
        ["repository"] = policy["repository"]!.DeepClone(), ["actor"] = label["actor"]!["login"]!.DeepClone(),
        ["event"] = "issues", ["action"] = label["event"]!.DeepClone(), ["label"] = label["label"]!["name"]!.DeepClone(),
        ["eventId"] = label["id"]!.DeepClone(), ["state"] = issue["state"]!.DeepClone(), ["isPullRequest"] = issue.ContainsKey("pull_request"),
        ["labels"] = new JsonArray(issue["labels"]!.AsArray().Select(item => item!["name"]!.DeepClone()).ToArray())
    };
    var decision = await lighthouse.Route(facts, token);
    if (decision["action"]?.GetValue<string>() != "run") throw new Rejected(decision["reason"]!.GetValue<string>());
    request["stage"] = decision["stage"]!.DeepClone(); request["route"] = decision["route"]!.DeepClone(); request["authorization"] = decision["authorization"]!.DeepClone();
    await github.CheckLabelAuthorization(request, token);
    return Results.Json(decision);
});
app.MapPost("/runs", async (JsonObject request, RunStore store, Lighthouse lighthouse, Github github, Supervisor supervisor, CancellationToken token) =>
{
    if (!supervisor.Ready) return Results.StatusCode(503);
    var raw = request.ToJsonString();
    // Exact retries retain their original deadline and return durable state even after expiry.
    if (request["runId"] is JsonValue id && id.TryGetValue<string>(out var key) && await store.Get(key, token) is { } previous)
    {
        if (previous.Request != raw) throw new Rejected("Idempotency identity was reused with a different request");
        return Results.Json(previous);
    }
    await lighthouse.Validate("request", raw, "", token);
    await github.CheckSource(request, token);
    await github.CheckApproval(request, store, token);
    return Results.Json(await store.Admit(raw, token));
});
app.MapGet("/runs/{id}", async (string id, RunStore store, CancellationToken token) => await store.Get(id, token) is { } run ? Results.Json(run) : Results.NotFound());
app.MapPost("/runs/{id}/cancel", async (string id, RunStore store, CancellationToken token) =>
{
    var run = await store.Cancel(id, token);
    return run is null ? Results.NotFound() : Results.Json(run);
});
await app.StartAsync();
File.SetUnixFileMode(options.Socket, UnixFileMode.UserRead | UnixFileMode.UserWrite | UnixFileMode.GroupRead | UnixFileMode.GroupWrite);
try { await app.WaitForShutdownAsync(); }
finally { File.Delete(options.Socket); }

public partial class Program;
