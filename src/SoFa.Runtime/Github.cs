using System.Formats.Tar;
using System.IO.Compression;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json.Nodes;

namespace SoFa.Runtime;

public sealed class Github(RuntimeOptions options, Lighthouse lighthouse, HttpClient http)
{
    async Task<HttpRequestMessage> Request(HttpMethod method, string path, CancellationToken token)
    {
        var request = new HttpRequestMessage(method, $"https://api.github.com/{path}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", (await File.ReadAllTextAsync(options.GithubTokenFile, token)).Trim());
        request.Headers.UserAgent.ParseAdd("SoFa/0.1");
        request.Headers.Accept.ParseAdd("application/vnd.github+json");
        return request;
    }
    public async Task<JsonObject> Get(string path, CancellationToken token)
    {
        using var request = await Request(HttpMethod.Get, path, token);
        using var response = await http.SendAsync(request, token);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<JsonObject>(token))!;
    }
    public async Task<JsonObject> CheckSource(JsonObject request, CancellationToken token)
    {
        var policy = await lighthouse.Policy(token);
        var repository = policy["repository"]!.GetValue<string>();
        if (request["policyDigest"]!.GetValue<string>() != Lighthouse.Hash(await File.ReadAllTextAsync(options.PolicyFile, token))) throw new Rejected("Policy changed");
        var repo = await Get($"repos/{repository}", token);
        var branch = await Get($"repos/{repository}/commits/{repo["default_branch"]!.GetValue<string>()}", token);
        if (branch["sha"]!.GetValue<string>() != request["sourceRevision"]!.GetValue<string>()) throw new Rejected("Source revision changed");
        var adoptionFile = await Get($"repos/{repository}/contents/.lighthouse/sdlc.json?ref={request["sourceRevision"]!.GetValue<string>()}", token);
        if (adoptionFile["encoding"]?.GetValue<string>() != "base64") throw new Rejected("Consumer adoption record is unavailable");
        var adoption = JsonNode.Parse(System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(adoptionFile["content"]!.GetValue<string>())))!;
        if (adoption["package"]?["name"]?.GetValue<string>() != "@juangcarmona/lighthouse-sdlc" ||
            adoption["package"]?["version"]?.GetValue<string>() != request["packageVersion"]!.GetValue<string>())
            throw new Rejected("Consumer adoption version does not match the admitted contract");
        var owner = policy["owner"]!.GetValue<string>();
        var identity = await Get("user", token);
        if (identity["login"]!.GetValue<string>() != owner) throw new Rejected("Publisher identity does not match the registered owner");
        var workflow = await Get($"repos/{repository}/actions/runs/{request["workflowRunId"]!.GetValue<long>()}", token);
        if (workflow["head_sha"]?.GetValue<string>() != request["workflowRevision"]!.GetValue<string>() ||
            workflow["path"]?.GetValue<string>() != ".github/workflows/lighthouse-host.yml" ||
            workflow["actor"]?["login"]?.GetValue<string>() != owner ||
            workflow["triggering_actor"]?["login"]?.GetValue<string>() != owner ||
            workflow["status"]?.GetValue<string>() != "in_progress" ||
            workflow["event"]?.GetValue<string>() is not ("issues" or "issue_comment" or "workflow_dispatch"))
            throw new Rejected("Workflow run does not attest the registered owner, entrypoint and revision");
        var issue = await Get($"repos/{repository}/issues/{request["issue"]!.GetValue<long>()}", token);
        if (issue.ContainsKey("pull_request") || issue["state"]!.GetValue<string>() != "open") throw new Rejected("Work item must be an open issue");
        if (issue["user"]!["login"]!.GetValue<string>() != policy["owner"]!.GetValue<string>()) throw new Rejected("Pilot intake accepts only owner-authored issues");
        return issue;
    }
    public async Task<JsonObject?> CheckApproval(JsonObject request, RunStore store, CancellationToken token)
    {
        if (request["stage"]!.GetValue<string>() != "refine") return null;
        if (request["schemaVersion"]?.GetValue<int>() == 2) return await CheckLabelAuthorization(request, token);
        var previous = await store.Get(request["predecessor"]!["runId"]!.GetValue<string>(), token) ?? throw new Rejected("Predecessor is missing");
        if (previous.State != "waiting-human" || previous.Result is null || previous.Receipt is null) throw new Rejected("Predecessor is not a published result");
        var priorRequest = JsonNode.Parse(previous.Request)!;
        foreach (var field in new[] { "repository", "issue", "sourceRevision", "policyDigest", "packageVersion", "workflowRevision" })
            if (!JsonNode.DeepEquals(request[field], priorRequest[field])) throw new Rejected("Predecessor authority changed");
        if (Lighthouse.Hash(previous.Result) != request["predecessor"]!["resultDigest"]!.GetValue<string>() || JsonNode.Parse(previous.Result)!["result"]!["status"]!.GetValue<string>() != "passed" || priorRequest["stage"]!.GetValue<string>() != "triage") throw new Rejected("Predecessor did not pass triage");
        var policy = await lighthouse.Policy(token);
        var comment = await Get($"repos/{previous.Repository}/issues/comments/{request["approvalCommentId"]!.GetValue<long>()}", token);
        var owner = policy["owner"]!.GetValue<string>();
        var issueUrl = $"https://api.github.com/repos/{previous.Repository}/issues/{previous.Issue}";
        if (comment["user"]!["login"]!.GetValue<string>() != owner || comment["body"]!.GetValue<string>() != $"/lh:refine {previous.Id}" || comment["issue_url"]!.GetValue<string>() != issueUrl) throw new Rejected("No matching owner decision for this predecessor");
        var receipt = new Uri(previous.Receipt);
        if (!receipt.Fragment.StartsWith("#issuecomment-", StringComparison.Ordinal) || !long.TryParse(receipt.Fragment[14..], out var receiptId)) throw new Rejected("Predecessor receipt is not an issue comment");
        var publication = await Get($"repos/{previous.Repository}/issues/comments/{receiptId}", token);
        if (publication["html_url"]?.GetValue<string>() != previous.Receipt || publication["issue_url"]?.GetValue<string>() != issueUrl || publication["user"]?["login"]?.GetValue<string>() != owner || previous.Intent is null || publication["body"]?.GetValue<string>() != previous.Intent)
            throw new Rejected("Predecessor publication no longer matches its receipt");
        if (!DateTimeOffset.TryParse(comment["created_at"]?.GetValue<string>(), out var approvedAt) || !DateTimeOffset.TryParse(publication["created_at"]?.GetValue<string>(), out var publishedAt) || approvedAt < publishedAt)
            throw new Rejected("Owner decision must follow predecessor publication");
        return new JsonObject
        {
            ["status"] = "verified", ["verifiedBy"] = "SoFa", ["authorizedStage"] = "refine",
            ["owner"] = owner, ["commentId"] = request["approvalCommentId"]!.DeepClone(),
            ["commentUrl"] = comment["html_url"]?.DeepClone(), ["body"] = comment["body"]!.DeepClone(),
            ["approvedAt"] = approvedAt.ToString("O"), ["predecessorPublishedAt"] = publishedAt.ToString("O"),
            ["predecessorRunId"] = previous.Id, ["predecessorResultDigest"] = Lighthouse.Hash(previous.Result),
            ["predecessorReceipt"] = previous.Receipt
        };
    }
    public static bool HasLabel(JsonObject issue, string name) => issue["labels"]?.AsArray().Any(label => label?["name"]?.GetValue<string>() == name) == true;
    public async Task<JsonObject> LabelEvent(JsonObject request, CancellationToken token)
    {
        var policy = await lighthouse.Policy(token);
        var repository = policy["repository"]!.GetValue<string>();
        JsonObject? latest = null;
        for (var page = 1; page <= 20; page++)
        {
            using var message = await Request(HttpMethod.Get, $"repos/{repository}/issues/{request["issue"]!.GetValue<long>()}/events?per_page=100&page={page}", token);
            using var response = await http.SendAsync(message, token);
            response.EnsureSuccessStatusCode();
            var events = (await response.Content.ReadFromJsonAsync<JsonArray>(token))!;
            foreach (var item in events.OfType<JsonObject>())
                if (item["event"]?.GetValue<string>() is "labeled" or "unlabeled" && item["label"]?["name"]?.GetValue<string>() == "refine") latest = item;
            if (events.Count < 100)
            {
                if (latest?["event"]?.GetValue<string>() != "labeled" || latest["actor"]?["login"]?.GetValue<string>() != policy["owner"]!.GetValue<string>()) throw new Rejected("No current owner refine label event");
                return latest;
            }
        }
        throw new Rejected("Issue event history exceeds the admission bound");
    }
    public async Task<JsonObject> CheckLabelAuthorization(JsonObject request, CancellationToken token)
    {
        var policy = await lighthouse.Policy(token);
        if (policy["directRefine"]?.GetValue<bool>() != true) throw new Rejected("Direct refinement is disabled");
        var repository = policy["repository"]!.GetValue<string>();
        var issue = await Get($"repos/{repository}/issues/{request["issue"]!.GetValue<long>()}", token);
        if (!HasLabel(issue, "refine") || HasLabel(issue, "human-working")) throw new Rejected("Refinement was revoked or reserved by a human");
        var label = await LabelEvent(request, token);
        if (label["id"]!.GetValue<long>() != request["authorization"]!["eventId"]!.GetValue<long>() || request["authorization"]!["actor"]!.GetValue<string>() != policy["owner"]!.GetValue<string>()) throw new Rejected("Refinement label authority changed");
        var workflow = await Get($"repos/{repository}/actions/runs/{request["workflowRunId"]!.GetValue<long>()}", token);
        if (workflow["event"]?.GetValue<string>() != "issues" || !DateTimeOffset.TryParse(workflow["created_at"]?.GetValue<string>(), out var started) || !DateTimeOffset.TryParse(label["created_at"]?.GetValue<string>(), out var labeled) || started < labeled || started - labeled > TimeSpan.FromMinutes(2)) throw new Rejected("Workflow does not correspond to the current label event");
        return new JsonObject { ["status"] = "verified", ["verifiedBy"] = "SoFa", ["authorizedStage"] = "refine", ["entry"] = "engineer", ["owner"] = policy["owner"]!.DeepClone(), ["labelEventId"] = label["id"]!.DeepClone() };
    }
    public async Task SetLabel(Run run, string label, bool present, CancellationToken token)
    {
        using var request = await Request(present ? HttpMethod.Post : HttpMethod.Delete, $"repos/{run.Repository}/issues/{run.Issue}/labels{(present ? "" : "/" + Uri.EscapeDataString(label))}", token);
        if (present) request.Content = JsonContent.Create(new { labels = new[] { label } });
        using var response = await http.SendAsync(request, token);
        if (present || response.StatusCode != System.Net.HttpStatusCode.NotFound) response.EnsureSuccessStatusCode();
        var fresh = await Get($"repos/{run.Repository}/issues/{run.Issue}", token);
        if (HasLabel(fresh, label) != present) throw new Rejected("Label projection could not be confirmed");
    }
    public async Task ReleaseWork(Run run, CancellationToken token)
    {
        if (run.Receipt is not null && JsonNode.Parse(run.Request)?["schemaVersion"]?.GetValue<int>() == 2)
        {
            var request = JsonNode.Parse(run.Request)!.AsObject();
            var issue = await Get($"repos/{run.Repository}/issues/{run.Issue}", token);
            JsonObject? current = null;
            if (HasLabel(issue, "refine") && !HasLabel(issue, "human-working"))
            {
                try { current = await LabelEvent(request, token); }
                catch (Rejected) { /* Revoked work does not project an outcome onto a newer request. */ }
            }
            if (current?["id"]?.GetValue<long>() == request["authorization"]!["eventId"]!.GetValue<long>())
            {
                var passed = JsonNode.Parse(run.Result!)!["result"]!["status"]!.GetValue<string>() == "passed";
                await SetLabel(run, passed ? "ready" : "needs-info", true, token);
                await SetLabel(run, passed ? "needs-info" : "ready", false, token);
                await SetLabel(run, "refine", false, token);
            }
        }
        await SetLabel(run, "agent-working", false, token);
    }
    public static string IssueDigest(JsonObject issue) => Lighthouse.Hash(new JsonObject { ["title"] = issue["title"]?.DeepClone(), ["body"] = issue["body"]?.DeepClone(), ["state"] = issue["state"]?.DeepClone() }.ToJsonString());
    public async Task Snapshot(string repository, string revision, string target, CancellationToken token)
    {
        using var request = await Request(HttpMethod.Get, $"repos/{repository}/tarball/{revision}", token);
        using var response = await http.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, token);
        response.EnsureSuccessStatusCode();
        await using var content = await response.Content.ReadAsStreamAsync(token);
        await using var gzip = new GZipStream(content, CompressionMode.Decompress);
        using var tar = new TarReader(gzip);
        void SourceDirectory(string path)
        {
            Directory.CreateDirectory(path);
            for (var dir = path; dir.StartsWith(Path.GetFullPath(target), StringComparison.Ordinal); dir = Path.GetDirectoryName(dir)!)
                if (OperatingSystem.IsLinux()) File.SetUnixFileMode(dir, (UnixFileMode)493);
        }
        long total = 0; int count = 0;
        while (await tar.GetNextEntryAsync(cancellationToken: token) is { } entry)
        {
            if (++count > 20000 || (total += entry.Length) > 100_000_000) throw new Rejected("Source snapshot exceeds pilot bounds");
            var slash = entry.Name.IndexOf('/');
            if (slash < 0 || slash == entry.Name.Length - 1) continue;
            var relative = entry.Name[(slash + 1)..];
            var file = Path.GetFullPath(Path.Combine(target, relative));
            if (!file.StartsWith(Path.GetFullPath(target) + Path.DirectorySeparatorChar, StringComparison.Ordinal)) throw new Rejected("Unsafe archive path");
            if (entry.EntryType == TarEntryType.Directory) { SourceDirectory(file); continue; }
            if (entry.EntryType is TarEntryType.SymbolicLink or TarEntryType.HardLink) continue;
            if (entry.EntryType is not (TarEntryType.RegularFile or TarEntryType.V7RegularFile)) throw new Rejected("Unsupported source archive entry");
            SourceDirectory(Path.GetDirectoryName(file)!);
            await using var output = File.Create(file);
            if (OperatingSystem.IsLinux()) File.SetUnixFileMode(file, UnixFileMode.UserRead | UnixFileMode.UserWrite | UnixFileMode.GroupRead | UnixFileMode.OtherRead);
            if (entry.DataStream is not null) await entry.DataStream.CopyToAsync(output, token);
        }
    }
    public static string Marker(string id) => $"<!-- lighthouse-run:{id} -->";
    public async Task<string?> FindReceipt(Run run, CancellationToken token)
    {
        var policy = await lighthouse.Policy(token);
        for (var page = 1; page <= 100; page++)
        {
            using var request = await Request(HttpMethod.Get, $"repos/{run.Repository}/issues/{run.Issue}/comments?per_page=100&page={page}", token);
            using var response = await http.SendAsync(request, token);
            response.EnsureSuccessStatusCode();
            var comments = (await response.Content.ReadFromJsonAsync<JsonArray>(token))!;
            foreach (var comment in comments)
            {
                if (comment?["user"]?["login"]?.GetValue<string>() != policy["owner"]!.GetValue<string>()) continue;
                if (comment?["body"]?.GetValue<string>() == run.Intent) return comment!["html_url"]!.GetValue<string>();
                if (comment?["body"]?.GetValue<string>().Contains(Marker(run.Id), StringComparison.Ordinal) == true) throw new Rejected("Existing publication marker has different content; owner reconciliation is required");
            }
            if (comments.Count < 100) return null;
        }
        throw new Rejected("Publication reconciliation exceeded the comment scan bound");
    }
    public async Task<string> Publish(Run run, CancellationToken token)
    {
        var existing = await FindReceipt(run, token);
        if (existing is not null) return existing;
        using var request = await Request(HttpMethod.Post, $"repos/{run.Repository}/issues/{run.Issue}/comments", token);
        request.Content = JsonContent.Create(new { body = run.Intent });
        using var response = await http.SendAsync(request, token);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<JsonObject>(token))!["html_url"]!.GetValue<string>();
    }
}
