using Npgsql;
using SoFa.Runtime;
using System.Text.Json;

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
