using System.Net;
using System.Text.Json.Nodes;
using SoFa.Runtime;

namespace SoFa.Runtime.Tests;

public sealed class GithubTests : IDisposable
{
    readonly string directory = Path.Combine(Path.GetTempPath(), $"sofa-github-{Guid.NewGuid():N}");
    readonly RuntimeOptions options;
    readonly JsonObject policy = new() { ["repository"] = "owner/repo", ["owner"] = "owner" };
    public GithubTests()
    {
        Directory.CreateDirectory(directory);
        options = new() { Socket = "unused", ClientUid = 1, PolicyFile = Path.Combine(directory, "policy.json"), ContractRoot = "unused", WorkRoot = directory, GithubTokenFile = Path.Combine(directory, "token"), EngineImage = "unused", EngineAuthVolume = "unused", Model = "unused" };
        File.WriteAllText(options.PolicyFile, policy.ToJsonString()); File.WriteAllText(options.GithubTokenFile, "test-only");
    }
    public void Dispose() => Directory.Delete(directory, true);
    sealed class Handler(Func<HttpRequestMessage, HttpResponseMessage> respond) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken token) => Task.FromResult(respond(request));
    }
    static HttpResponseMessage Json(string body) => new(HttpStatusCode.OK) { Content = new StringContent(body, System.Text.Encoding.UTF8, "application/json") };
    Github Client(Func<HttpRequestMessage, HttpResponseMessage> respond) => new(options, new Lighthouse(options), new HttpClient(new Handler(respond)));
    [Theory]
    [InlineData("wrong-sha", "in_progress", "owner", false)]
    [InlineData("revision", "completed", "owner", false)]
    [InlineData("revision", "in_progress", "stranger", false)]
    [InlineData("revision", "in_progress", "owner", true)]
    public async Task AdmissionRequiresCurrentSourceAndAttestedWorkflow(string sha, string state, string actor, bool valid)
    {
        var github = Client(request => request.RequestUri!.AbsolutePath switch
        {
            "/user" => Json("{\"login\":\"owner\"}"),
            "/repos/owner/repo" => Json("{\"default_branch\":\"main\"}"),
            "/repos/owner/repo/commits/main" => Json("{\"sha\":\"revision\"}"),
            "/repos/owner/repo/actions/runs/1" => Json(new JsonObject { ["head_sha"] = sha, ["status"] = state, ["actor"] = new JsonObject { ["login"] = actor }, ["triggering_actor"] = new JsonObject { ["login"] = actor }, ["path"] = ".github/workflows/lighthouse-host.yml", ["event"] = "issues" }.ToJsonString()),
            "/repos/owner/repo/issues/1" => Json("{\"state\":\"open\",\"user\":{\"login\":\"owner\"}}"),
            _ => throw new Exception("Unexpected GitHub request")
        });
        var input = new JsonObject { ["sourceRevision"] = "revision", ["workflowRevision"] = "revision", ["workflowRunId"] = 1L, ["issue"] = 1L, ["policyDigest"] = Lighthouse.Hash(policy.ToJsonString()) };
        if (valid) Assert.NotNull(await github.CheckSource(input, CancellationToken.None));
        else await Assert.ThrowsAsync<Rejected>(() => github.CheckSource(input, CancellationToken.None));
    }
    [Fact]
    public async Task ReconciliationRequiresExactIntentAndPublisherIdentity()
    {
        var run = new Run("id", "owner/repo", 1, "{}", "publishing", 1, DateTime.UtcNow.AddMinutes(1), "result", "body\n" + Github.Marker("id"), null, null);
        JsonObject Comment(string owner, string body) => new() { ["user"] = new JsonObject { ["login"] = owner }, ["body"] = body, ["html_url"] = "https://github.com/receipt" };
        var comments = new JsonArray(Comment("attacker", run.Intent!));
        var github = Client(_ => Json(comments.ToJsonString()));
        Assert.Null(await github.FindReceipt(run, CancellationToken.None));
        comments.Add(Comment("owner", run.Intent!));
        Assert.Equal("https://github.com/receipt", await github.FindReceipt(run, CancellationToken.None));
        comments.Clear(); comments.Add(Comment("owner", "modified " + Github.Marker("id")));
        await Assert.ThrowsAsync<Rejected>(() => github.FindReceipt(run, CancellationToken.None));
    }
}
