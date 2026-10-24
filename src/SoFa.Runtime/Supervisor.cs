using Npgsql;
using System.Text.Json.Nodes;

namespace SoFa.Runtime;

public sealed class Supervisor(RunStore store, Lighthouse lighthouse, Github github, Engine engine, RuntimeOptions options, NpgsqlDataSource database, IHostApplicationLifetime lifetime, ILogger<Supervisor> logger) : BackgroundService
{
    public bool Ready { get; private set; }
    protected override async Task ExecuteAsync(CancellationToken stopping)
    {
        try
        {
            await using var leader = await database.OpenConnectionAsync(stopping);
            await using var acquire = new NpgsqlCommand("SELECT pg_try_advisory_lock(7416002)", leader);
            if (await acquire.ExecuteScalarAsync(stopping) is not true) throw new Rejected("Another supervisor holds leadership");
            await store.Initialize(stopping);
            foreach (var run in await store.Unfinished(stopping))
            {
                await engine.Stop(run.Id, stopping);
                if (run.State is "publishing" or "publication-uncertain")
                {
                    var receipt = await github.FindReceipt(run, stopping);
                    if (receipt is not null) await store.Transition(run, run.State, "waiting-human", stopping, receipt: receipt);
                    else await store.Transition(run, run.State, "publication-uncertain", stopping, error: "No confirmed remote receipt; owner reconciliation is required before admitting more work");
                }
                else await store.Interrupt(run, stopping);
            }
            Ready = true;
            while (!stopping.IsCancellationRequested)
            {
                await using var heartbeat = new NpgsqlCommand("SELECT 1", leader);
                await heartbeat.ExecuteScalarAsync(stopping);
                var run = await store.Claim(stopping);
                if (run is null) { await Task.Delay(300, stopping); continue; }
                await Execute(run, leader, stopping);
            }
        }
        catch (OperationCanceledException) when (stopping.IsCancellationRequested) { }
        catch (Exception error) { logger.LogError("Supervisor stopped: {Type}", error.GetType().Name); lifetime.StopApplication(); }
        finally { Ready = false; }
    }
    async Task Execute(Run run, NpgsqlConnection leader, CancellationToken stopping)
    {
        var dir = Path.Combine(options.WorkRoot, "runs", run.Id);
        var publishing = false;
        using var deadline = CancellationTokenSource.CreateLinkedTokenSource(stopping);
        deadline.CancelAfter(run.Deadline > DateTime.UtcNow ? run.Deadline - DateTime.UtcNow : TimeSpan.Zero);
        using var monitorStop = CancellationTokenSource.CreateLinkedTokenSource(stopping);
        async Task Monitor()
        {
            try
            {
                while (!monitorStop.IsCancellationRequested)
                {
                    await Task.Delay(500, monitorStop.Token);
                    var current = await store.Get(run.Id, monitorStop.Token);
                    await using var heartbeat = new NpgsqlCommand("SELECT 1", leader);
                    await heartbeat.ExecuteScalarAsync(monitorStop.Token);
                    if (current?.Fence != run.Fence || current.State == "cancelling") { deadline.Cancel(); break; }
                }
            }
            catch (OperationCanceledException) when (monitorStop.IsCancellationRequested) { }
            catch { deadline.Cancel(); }
        }
        var monitor = Monitor();
        try
        {
            var token = deadline.Token;
            await lighthouse.Validate("request", run.Request, "", token);
            var request = JsonNode.Parse(run.Request)!.AsObject();
            var issue = await github.CheckSource(request, token);
            await github.CheckApproval(request, store, token);
            var issueDigest = Github.IssueDigest(issue);
            Directory.CreateDirectory(dir);
            if (OperatingSystem.IsLinux()) File.SetUnixFileMode(dir, UnixFileMode.UserRead | UnixFileMode.UserWrite | UnixFileMode.UserExecute);
            var source = Path.Combine(dir, "source"); var output = Path.Combine(dir, "output");
            Directory.CreateDirectory(source); Directory.CreateDirectory(output);
            if (OperatingSystem.IsLinux()) File.SetUnixFileMode(source, (UnixFileMode)493);
            if (OperatingSystem.IsLinux()) File.SetUnixFileMode(output, (UnixFileMode)511);
            await github.Snapshot(run.Repository, request["sourceRevision"]!.GetValue<string>(), source, token);
            var requestFile = Path.Combine(dir, "request.json"); var issueFile = Path.Combine(dir, "issue.json");
            await File.WriteAllTextAsync(requestFile, run.Request, token);
            await File.WriteAllTextAsync(issueFile, issue.ToJsonString(), token);
            string? predecessorFile = null;
            if (request["predecessor"] is JsonObject predecessor)
            {
                predecessorFile = Path.Combine(dir, "predecessor.json");
                await File.WriteAllTextAsync(predecessorFile, (await store.Get(predecessor["runId"]!.GetValue<string>(), token))!.Result!, token);
            }
            var context = Path.Combine(dir, "context.json");
            await File.WriteAllTextAsync(context, await lighthouse.Context(requestFile, issueFile, predecessorFile, token), token);
            if (OperatingSystem.IsLinux()) File.SetUnixFileMode(context, UnixFileMode.UserRead | UnixFileMode.GroupRead | UnixFileMode.OtherRead);
            var envelope = await engine.Execute(run, source, output, context, token);
            await lighthouse.Validate("result", run.Request, envelope, token);
            var value = JsonNode.Parse(envelope)!;
            if (value["result"]!["status"]!.GetValue<string>() == "failed")
            {
                await store.Transition(run, "running", "failed", token, result: envelope, error: "Stage reported failure"); return;
            }
            var freshIssue = await github.CheckSource(request, token);
            await github.CheckApproval(request, store, token);
            if (Github.IssueDigest(freshIssue) != issueDigest) throw new Rejected("Issue changed during execution");
            var body = $"{value["effect"]!["body"]!.GetValue<string>()}\n\n{Github.Marker(run.Id)}\n\nRun `{run.Id}` · source `{request["sourceRevision"]!.GetValue<string>()}` · result `{Lighthouse.Hash(envelope)}`. Awaiting owner decision.";
            if (!await store.Transition(run, "running", "publishing", token, result: envelope, intent: body)) throw new Rejected("Execution authority changed before publication");
            publishing = true;
            var withIntent = run with { State = "publishing", Result = envelope, Intent = body };
            await store.PublishFenced(run, () => github.Publish(withIntent, token), token);
        }
        catch (Exception error)
        {
            using var cleanup = new CancellationTokenSource(TimeSpan.FromSeconds(30));
            await engine.Stop(run.Id, cleanup.Token);
            var current = await store.Get(run.Id, cleanup.Token);
            if (current is not null && current.Fence == run.Fence)
            {
                var state = publishing ? "publication-uncertain" : current.State == "cancelling" ? "cancelled" : deadline.IsCancellationRequested ? "timed-out" : "failed";
                if (current.State is "running" or "publishing" or "cancelling") await store.Transition(run, current.State, state, cleanup.Token, error: error is Rejected ? error.Message : error.GetType().Name);
            }
        }
        finally
        {
            monitorStop.Cancel(); await monitor;
            if (Directory.Exists(dir)) Directory.Delete(dir, true);
        }
    }
}
