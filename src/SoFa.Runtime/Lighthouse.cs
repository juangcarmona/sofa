using System.Security.Cryptography;
using System.Text;
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
            await ProcessRunner.Run("node", [Path.Combine(options.ContractRoot, "src/host-contract.mjs"), command, input, second], token);
        }
        finally { Directory.Delete(dir, true); }
    }
    public Task<string> Context(string requestFile, string issueFile, string? predecessorFile, CancellationToken token) =>
        ProcessRunner.Run("node", new[] { Path.Combine(options.ContractRoot, "src/host-contract.mjs"), "context", requestFile, issueFile }.Concat(predecessorFile is null ? [] : new[] { predecessorFile }), token);
}
