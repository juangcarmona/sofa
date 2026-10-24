using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace SoFa.Runtime;

public sealed class Lighthouse(RuntimeOptions options)
{
    public static string Hash(string text) => Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(text)));
    public async Task<JsonObject> Policy(CancellationToken token) => JsonNode.Parse(await File.ReadAllTextAsync(options.PolicyFile, token))!.AsObject();
    public async Task Validate(string command, string request, string extra, CancellationToken token)
    {
        var dir = Path.Combine(options.WorkRoot, "validation", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(dir);
        try
        {
            var input = Path.Combine(dir, "request.json");
            var second = command == "request" ? options.PolicyFile : Path.Combine(dir, "result.json");
            await File.WriteAllTextAsync(input, request, token);
            if (command != "request") await File.WriteAllTextAsync(second, extra, token);
            string output;
            var succeeded = true;
            try { output = await ProcessRunner.Run("node", [Path.Combine(options.ContractRoot, "src/host-contract.mjs"), command, input, second], token, maxOutput: 16384); }
            catch (ProcessFailure error) when (error.ExitCode == 1) { output = error.Output; succeeded = false; }
            ValidationReport? report;
            try { report = JsonSerializer.Deserialize<ValidationReport>(output, JsonSerializerOptions.Web); }
            catch (JsonException) { throw new Rejected($"Lighthouse {command} validator returned no structured report"); }
            if (report is null || report.SchemaVersion != 1 || report.Errors is null || report.Errors.Any(string.IsNullOrWhiteSpace) || report.Valid != (report.Errors.Length == 0) || report.Valid != succeeded)
                throw new Rejected($"Lighthouse {command} validator returned an inconsistent report");
            if (!report.Valid) throw new Rejected($"Lighthouse {command} validation: {string.Join("; ", report.Errors.Take(8).Select(error => error[..Math.Min(error.Length, 256)]))}");
        }
        finally { Directory.Delete(dir, true); }
    }
    sealed record ValidationReport(int SchemaVersion, bool Valid, string[] Errors);
    public Task<string> Context(string requestFile, string issueFile, string? predecessorFile, CancellationToken token) =>
        ProcessRunner.Run("node", new[] { Path.Combine(options.ContractRoot, "src/host-contract.mjs"), "context", requestFile, issueFile }.Concat(predecessorFile is null ? [] : new[] { predecessorFile }), token);
}
