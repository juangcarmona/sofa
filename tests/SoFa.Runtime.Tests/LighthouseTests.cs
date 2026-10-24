using System.Text.Json;
using SoFa.Runtime;

namespace SoFa.Runtime.Tests;

public sealed class LighthouseTests : IDisposable
{
    readonly string directory = Path.Combine(Path.GetTempPath(), $"sofa-validator-{Guid.NewGuid():N}");
    readonly RuntimeOptions options;
    public LighthouseTests()
    {
        Directory.CreateDirectory(Path.Combine(directory, "src"));
        options = new() { Socket = "unused", ClientUid = 1, PolicyFile = "unused", ContractRoot = directory, WorkRoot = directory, GithubTokenFile = "unused", EngineImage = "unused", EngineAuthVolume = "unused", Model = "unused" };
    }
    public void Dispose() => Directory.Delete(directory, true);
    void Validator(string output, int exitCode) => File.WriteAllText(Path.Combine(directory, "src/host-contract.mjs"),
        $"console.log({JsonSerializer.Serialize(output)}); console.error('private-process-output'); process.exitCode = {exitCode};");

    [Fact]
    public async Task RejectedResultKeepsTheBoundedContractReasonWithoutProcessLogs()
    {
        Validator("""{"schemaVersion":1,"valid":false,"errors":["result identity or gate does not match admission"]}""", 1);
        var error = await Assert.ThrowsAsync<Rejected>(() => new Lighthouse(options).Validate("result", "{}", "{}", CancellationToken.None));
        Assert.Contains("Lighthouse result validation: result identity or gate does not match admission", error.Message);
        Assert.DoesNotContain("private-process-output", error.ToString());
        Assert.Empty(Directory.GetDirectories(Path.Combine(directory, "validation")));
    }

    [Theory]
    [InlineData("{\"schemaVersion\":1,\"valid\":true,\"errors\":[]}", 0, true)]
    [InlineData("{\"schemaVersion\":1,\"valid\":true,\"errors\":[]}", 1, false)]
    [InlineData("{\"schemaVersion\":1,\"valid\":false,\"errors\":[\"bad result\"]}", 0, false)]
    [InlineData("private-process-output", 1, false)]
    [InlineData("{}", 0, false)]
    [InlineData("{\"schemaVersion\":1,\"valid\":true,\"errors\":[]}", 2, false)]
    public async Task OnlyAConsistentSuccessfulReportPasses(string output, int exitCode, bool valid)
    {
        Validator(output, exitCode);
        var action = () => new Lighthouse(options).Validate("result", "{}", "{}", CancellationToken.None);
        if (valid) await action();
        else
        {
            var error = await Assert.ThrowsAnyAsync<Rejected>(action);
            Assert.DoesNotContain("private-process-output", error.ToString());
        }
    }

    [Fact]
    public async Task DiagnosticCountAndLengthStayBounded()
    {
        Validator(JsonSerializer.Serialize(new { schemaVersion = 1, valid = false, errors = Enumerable.Repeat(new string('x', 300), 10) }), 1);
        var error = await Assert.ThrowsAsync<Rejected>(() => new Lighthouse(options).Validate("result", "{}", "{}", CancellationToken.None));
        Assert.Equal("Lighthouse result validation: " + string.Join("; ", Enumerable.Repeat(new string('x', 256), 8)), error.Message);
    }
}
