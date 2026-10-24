using Npgsql;
using SoFa.Runtime;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Net;

namespace SoFa.Runtime.Tests;

public sealed class AdmissionTests : IAsyncLifetime
{
    NpgsqlDataSource database = null!;
    RunStore store = null!;
    readonly string schema = $"test_{Guid.NewGuid():N}";
    string connection = null!;
    public async Task InitializeAsync()
    {
        connection = Environment.GetEnvironmentVariable("SOFA_TEST_DATABASE") ?? throw new InvalidOperationException("Run npm run test:runtime to provision PostgreSQL");
        await using var admin = NpgsqlDataSource.Create(connection);
        await using var command = admin.CreateCommand($"CREATE SCHEMA {schema}");
        await command.ExecuteNonQueryAsync();
        database = NpgsqlDataSource.Create(new NpgsqlConnectionStringBuilder(connection) { SearchPath = schema }.ToString());
        store = new RunStore(database);
        await store.Initialize(CancellationToken.None);
    }
    public async Task DisposeAsync()
    {
        await database.DisposeAsync();
        await using var admin = NpgsqlDataSource.Create(connection);
        await using var command = admin.CreateCommand($"DROP SCHEMA {schema} CASCADE");
        await command.ExecuteNonQueryAsync();
    }
    static string Request(string id = "one", int issue = 1) => JsonSerializer.Serialize(new { runId = id, repository = "owner/repository", stage = "triage", sourceRevision = "revision", issue, deadline = DateTime.UtcNow.AddMinutes(5).ToString("O") });

    sealed class ApprovalHandler(Func<HttpRequestMessage, HttpResponseMessage> respond) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken token) => Task.FromResult(respond(request));
    }

    [Theory]
    [InlineData("valid")]
    [InlineData("wrong-owner")]
    [InlineData("wrong-command")]
    [InlineData("before-publication")]
    [InlineData("changed-publication")]
    [InlineData("wrong-predecessor-digest")]
    public async Task RefinementEvidenceRequiresMatchingOwnerAndPublishedPredecessor(string scenario)
    {
        var dir = Path.Combine(Path.GetTempPath(), $"sofa-approval-{Guid.NewGuid():N}");
        Directory.CreateDirectory(dir);
        try
        {
            var options = new RuntimeOptions { Socket = "unused", ClientUid = 1, PolicyFile = Path.Combine(dir, "policy.json"), ContractRoot = "unused", WorkRoot = dir, GithubTokenFile = Path.Combine(dir, "token"), EngineImage = "unused", EngineAuthVolume = "unused", Model = "unused" };
            await File.WriteAllTextAsync(options.PolicyFile, """{"owner":"owner"}""");
            await File.WriteAllTextAsync(options.GithubTokenFile, "test-only");
            var prior = JsonNode.Parse(Request())!.AsObject();
            prior["policyDigest"] = "policy"; prior["packageVersion"] = "0.9.0"; prior["workflowRevision"] = "revision";
            var run = await store.Admit(prior.ToJsonString(), CancellationToken.None);
            const string result = """{"result":{"status":"passed"}}""";
            const string receipt = "https://github.com/owner/repository/issues/1#issuecomment-10";
            const string issueUrl = "https://api.github.com/repos/owner/repository/issues/1";
            await store.Transition(run, "admitted", "waiting-human", CancellationToken.None, result: result, intent: "triage report", receipt: receipt);
            var input = prior.DeepClone().AsObject();
            input["stage"] = "refine"; input["approvalCommentId"] = 11L;
            input["predecessor"] = new JsonObject { ["runId"] = run.Id, ["resultDigest"] = scenario == "wrong-predecessor-digest" ? "wrong" : Lighthouse.Hash(result) };
            JsonObject Comment(string owner, string body, string url, string created) => new() { ["user"] = new JsonObject { ["login"] = owner }, ["body"] = body, ["html_url"] = url, ["issue_url"] = issueUrl, ["created_at"] = created };
            var approval = Comment(scenario == "wrong-owner" ? "stranger" : "owner", scenario == "wrong-command" ? "/lh:refine other" : $"/lh:refine {run.Id}", "https://github.com/owner/repository/issues/1#issuecomment-11", scenario == "before-publication" ? "2026-10-24T10:00:00Z" : "2026-10-24T10:02:00Z");
            var publication = Comment("owner", scenario == "changed-publication" ? "edited" : "triage report", receipt, "2026-10-24T10:01:00Z");
            using var http = new HttpClient(new ApprovalHandler(request => new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent((request.RequestUri!.AbsolutePath.EndsWith("/11", StringComparison.Ordinal) ? approval : publication).ToJsonString()) }));
            var github = new Github(options, new Lighthouse(options), http);
            if (scenario == "valid")
            {
                var evidence = (await github.CheckApproval(input, store, CancellationToken.None))!;
                Assert.Equal("verified", evidence["status"]!.GetValue<string>());
                Assert.Equal("refine", evidence["authorizedStage"]!.GetValue<string>());
                Assert.Equal(11L, evidence["commentId"]!.GetValue<long>());
                Assert.Equal($"/lh:refine {run.Id}", evidence["body"]!.GetValue<string>());
                Assert.Equal(Lighthouse.Hash(result), evidence["predecessorResultDigest"]!.GetValue<string>());
                Assert.Equal(receipt, evidence["predecessorReceipt"]!.GetValue<string>());
            }
            else await Assert.ThrowsAsync<Rejected>(() => github.CheckApproval(input, store, CancellationToken.None));
        }
        finally { Directory.Delete(dir, true); }
    }
    [Fact]
    public async Task ConcurrentDuplicatesReturnOneAttemptAndChangedPayloadFails()
    {
        var request = Request();
        var admitted = await Task.WhenAll(Enumerable.Range(0, 8).Select(_ => store.Admit(request, CancellationToken.None)));
        Assert.All(admitted, run => Assert.Equal(admitted[0], run));
        await Assert.ThrowsAsync<Rejected>(() => store.Admit(Request(issue: 2), CancellationToken.None));
        Assert.NotNull(await store.Claim(CancellationToken.None));
        Assert.Null(await store.Claim(CancellationToken.None));
    }
    [Fact]
    public async Task GlobalCapacityHoldsUnderContentionAndHumanWaitReleasesIt()
    {
        var attempts = await Task.WhenAll(Enumerable.Range(0, 8).Select(async i => { try { return await store.Admit(Request($"run-{i}", i + 1), CancellationToken.None); } catch (Rejected) { return null; } }));
        var admitted = Assert.Single(attempts, x => x is not null)!;
        Assert.True(await store.Transition(admitted, "admitted", "waiting-human", CancellationToken.None));
        Assert.NotNull(await store.Admit(Request("next", 9), CancellationToken.None));
    }
    [Fact]
    public async Task InterruptionChangesFenceAndRejectsLateWorkerWrites()
    {
        await store.Admit(Request(), CancellationToken.None);
        var running = (await store.Claim(CancellationToken.None))!;
        Assert.True(await store.Interrupt(running, CancellationToken.None));
        Assert.False(await store.Transition(running, "interrupted", "publishing", CancellationToken.None));
        var interrupted = (await store.Get(running.Id, CancellationToken.None))!;
        Assert.Equal(running.Fence + 1, interrupted.Fence);
        Assert.Equal("interrupted", interrupted.State);
    }
    [Fact]
    public async Task PublicationChecksFenceAndPersistsReceiptBeforeRelease()
    {
        await store.Admit(Request(), CancellationToken.None);
        var run = (await store.Claim(CancellationToken.None))!;
        Assert.True(await store.Transition(run, "running", "publishing", CancellationToken.None, result: "result", intent: "comment"));
        var calls = 0;
        Task<string> Publish() { calls++; return Task.FromResult("https://github.com/receipt"); }
        await Assert.ThrowsAsync<Rejected>(() => store.PublishFenced(run with { Fence = 0 }, Publish, CancellationToken.None));
        Assert.Equal(0, calls);
        await store.PublishFenced(run, Publish, CancellationToken.None);
        var done = (await store.Get(run.Id, CancellationToken.None))!;
        Assert.Equal("waiting-human", done.State);
        Assert.Equal("https://github.com/receipt", done.Receipt);
        await Assert.ThrowsAsync<Rejected>(() => store.PublishFenced(run, Publish, CancellationToken.None));
        Assert.Equal(1, calls);
    }
    [Fact]
    public async Task UncertainRemoteEffectRetainsIntentAndOccupiesCapacity()
    {
        await store.Admit(Request(), CancellationToken.None);
        var run = (await store.Claim(CancellationToken.None))!;
        await store.Transition(run, "running", "publishing", CancellationToken.None, intent: "stable marker");
        await Assert.ThrowsAsync<IOException>(() => store.PublishFenced(run, () => throw new IOException("remote reply lost"), CancellationToken.None));
        var uncertain = (await store.Get(run.Id, CancellationToken.None))!;
        Assert.Equal("publishing", uncertain.State);
        Assert.Equal("stable marker", uncertain.Intent);
        Assert.Null(uncertain.Receipt);
        await Assert.ThrowsAsync<Rejected>(() => store.Admit(Request("next"), CancellationToken.None));
    }
    [Fact]
    public async Task CancellationRevokesPublicationBeforeDispatch()
    {
        await store.Admit(Request(), CancellationToken.None);
        var run = (await store.Claim(CancellationToken.None))!;
        await store.Transition(run, "running", "cancelling", CancellationToken.None);
        Assert.False(await store.Transition(run, "running", "publishing", CancellationToken.None));
        await Assert.ThrowsAsync<Rejected>(() => store.PublishFenced(run, () => throw new Exception("must not be invoked"), CancellationToken.None));
    }
    [Fact]
    public async Task CancellationRacingClaimCannotLeaveAnActiveAttempt()
    {
        for (var i = 0; i < 12; i++)
        {
            var id = $"race-{i}";
            await store.Admit(Request(id, i + 1), CancellationToken.None);
            await Task.WhenAll(store.Claim(CancellationToken.None), store.Cancel(id, CancellationToken.None));
            var current = (await store.Get(id, CancellationToken.None))!;
            Assert.Contains(current.State, new[] { "cancelled", "cancelling" });
            if (current.State == "cancelling") await store.Transition(current, "cancelling", "cancelled", CancellationToken.None);
        }
    }
    [Fact]
    public async Task AttemptLimitCannotBeBypassedWithFreshEventIdentities()
    {
        for (var i = 0; i < 3; i++)
        {
            var run = await store.Admit(Request($"attempt-{i}"), CancellationToken.None);
            await store.Cancel(run.Id, CancellationToken.None);
        }
        await Assert.ThrowsAsync<Rejected>(() => store.Admit(Request("attempt-4"), CancellationToken.None));
        Assert.NotNull(await store.Admit(Request("other-issue", 2), CancellationToken.None));
    }

}
