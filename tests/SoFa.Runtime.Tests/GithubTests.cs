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
    [InlineData("wrong-sha", "in_progress", "owner", "0.9.0", false)]
    [InlineData("revision", "completed", "owner", "0.9.0", false)]
    [InlineData("revision", "in_progress", "stranger", "0.9.0", false)]
    [InlineData("revision", "in_progress", "owner", "0.8.0", false)]
    [InlineData("revision", "in_progress", "owner", "0.9.0", true)]
    public async Task AdmissionRequiresCurrentSourceAndAttestedWorkflow(string sha, string state, string actor, string adoptedVersion, bool valid)
    {
        var github = Client(request => request.RequestUri!.AbsolutePath switch
        {
            "/repos/owner/repo/contents/.lighthouse/sdlc.json" => Json(new JsonObject { ["encoding"] = "base64", ["content"] = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(new JsonObject { ["package"] = new JsonObject { ["name"] = "@juangcarmona/lighthouse-sdlc", ["version"] = adoptedVersion } }.ToJsonString())) }.ToJsonString()),
            "/user" => Json("{\"login\":\"owner\"}"),
            "/repos/owner/repo" => Json("{\"default_branch\":\"main\"}"),
            "/repos/owner/repo/commits/main" => Json("{\"sha\":\"revision\"}"),
            "/repos/owner/repo/actions/runs/1" => Json(new JsonObject { ["head_sha"] = sha, ["status"] = state, ["actor"] = new JsonObject { ["login"] = actor }, ["triggering_actor"] = new JsonObject { ["login"] = actor }, ["path"] = ".github/workflows/lighthouse-host.yml", ["event"] = "issues" }.ToJsonString()),
            "/repos/owner/repo/issues/1" => Json("{\"state\":\"open\",\"user\":{\"login\":\"owner\"}}"),
            _ => throw new Exception("Unexpected GitHub request")
        });
        var input = new JsonObject { ["packageVersion"] = "0.9.0", ["sourceRevision"] = "revision", ["workflowRevision"] = "revision", ["workflowRunId"] = 1L, ["issue"] = 1L, ["policyDigest"] = Lighthouse.Hash(policy.ToJsonString()) };
        if (valid) Assert.NotNull(await github.CheckSource(input, CancellationToken.None));
        else await Assert.ThrowsAsync<Rejected>(() => github.CheckSource(input, CancellationToken.None));
    }
    [Fact]
    public async Task LabelAuthorityRequiresOwnerLatestEventLiveLabelAndMatchingWorkflow()
    {
        policy["directRefine"] = true;
        await File.WriteAllTextAsync(options.PolicyFile, policy.ToJsonString());
        var label = new JsonObject { ["id"] = 42L, ["event"] = "labeled", ["label"] = new JsonObject { ["name"] = "refine" }, ["actor"] = new JsonObject { ["login"] = "owner" }, ["created_at"] = "2026-10-24T21:00:00Z" };
        var issue = JsonNode.Parse("""{"labels":[{"name":"refine"}]}""")!.AsObject();
        var workflow = JsonNode.Parse("""{"event":"issues","created_at":"2026-10-24T21:00:02Z"}""")!.AsObject();
        var github = Client(request => request.RequestUri!.AbsolutePath switch
        {
            "/repos/owner/repo/issues/1/events" => Json(new JsonArray(label.DeepClone()).ToJsonString()),
            "/repos/owner/repo/issues/1" => Json(issue.ToJsonString()),
            "/repos/owner/repo/actions/runs/7" => Json(workflow.ToJsonString()),
            _ => throw new Exception("Unexpected request")
        });
        var input = JsonNode.Parse("""{"schemaVersion":2,"stage":"refine","issue":1,"workflowRunId":7,"authorization":{"actor":"owner","eventId":42}}""")!.AsObject();
        Assert.Equal("engineer", (await github.CheckLabelAuthorization(input, CancellationToken.None))["entry"]!.GetValue<string>());
        foreach (var mutation in new Action[] {
            () => label["actor"]!["login"] = "stranger", () => label["event"] = "unlabeled",
            () => label["id"] = 43L, () => issue["labels"]!.AsArray().Add(new JsonObject { ["name"] = "human-working" }),
            () => issue["labels"]!.AsArray().Clear(), () => workflow["event"] = "issue_comment",
            () => workflow["created_at"] = "2026-10-24T20:59:59Z", () => workflow["created_at"] = "2026-10-24T21:03:00Z"
        })
        {
            var savedLabel = label.DeepClone().AsObject(); var savedIssue = issue.DeepClone().AsObject(); var savedWorkflow = workflow.DeepClone().AsObject();
            mutation();
            await Assert.ThrowsAsync<Rejected>(() => github.CheckLabelAuthorization(input, CancellationToken.None));
            label = savedLabel; issue = savedIssue; workflow = savedWorkflow;
        }
    }
    [Fact]
    public async Task ReservationCleanupProjectsOnlyItsOwnOutcomeAndPreservesHumanLabels()
    {
        var labels = new HashSet<string> { "refine", "agent-working", "unrelated" };
        var github = Client(message =>
        {
            var path = message.RequestUri!.AbsolutePath;
            if (path.EndsWith("/events", StringComparison.Ordinal)) return Json("""[{"id":42,"event":"labeled","label":{"name":"refine"},"actor":{"login":"owner"}}]""");
            if (message.Method == HttpMethod.Post)
                foreach (var label in JsonNode.Parse(message.Content!.ReadAsStringAsync().GetAwaiter().GetResult())!["labels"]!.AsArray()) labels.Add(label!.GetValue<string>());
            if (message.Method == HttpMethod.Delete) labels.Remove(path.Split('/').Last());
            return Json(new JsonObject { ["labels"] = new JsonArray(labels.Select(label => (JsonNode)new JsonObject { ["name"] = label }).ToArray()) }.ToJsonString());
        });
        var run = new Run("id", "owner/repo", 1, """{"schemaVersion":2,"issue":1,"authorization":{"eventId":42}}""", "waiting-human", 1, DateTime.UtcNow.AddMinutes(1), """{"result":{"status":"passed"}}""", "report", "receipt", null);
        await github.ReleaseWork(run, CancellationToken.None);
        Assert.True(labels.SetEquals(["ready", "unrelated"]));
        await github.ReleaseWork(run, CancellationToken.None);
        Assert.True(labels.SetEquals(["ready", "unrelated"]));
        labels = ["refine", "agent-working", "human-working", "unrelated"];
        await github.ReleaseWork(run, CancellationToken.None);
        Assert.True(labels.SetEquals(["refine", "human-working", "unrelated"]));
        labels = ["refine", "agent-working", "ready", "unrelated"];
        await github.ReleaseWork(run with { Result = """{"result":{"status":"blocked"}}""" }, CancellationToken.None);
        Assert.True(labels.SetEquals(["needs-info", "unrelated"]));
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
