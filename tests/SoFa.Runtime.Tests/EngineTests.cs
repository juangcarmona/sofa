using SoFa.Runtime;

namespace SoFa.Runtime.Tests;

public sealed class EngineTests : IAsyncLifetime
{
    readonly string directory = Path.Combine(Path.GetTempPath(), $"sofa-engine-{Guid.NewGuid():N}");
    readonly string image = $"sofa-test-engine:{Guid.NewGuid():N}";
    readonly string volume = $"sofa-test-auth-{Guid.NewGuid():N}";
    Engine engine = null!;
    string source = null!, output = null!, context = null!;
    public async Task InitializeAsync()
    {
        if (!OperatingSystem.IsLinux()) throw new PlatformNotSupportedException("Linux engine isolation tests require Linux");
        Directory.CreateDirectory(directory);
        source = Path.Combine(directory, "source"); output = Path.Combine(directory, "output"); context = Path.Combine(directory, "context.json");
        Directory.CreateDirectory(source); Directory.CreateDirectory(output);
        await File.WriteAllTextAsync(Path.Combine(source, "unchanged"), "original");
        await File.WriteAllTextAsync(context, "{}");
        File.SetUnixFileMode(source, (UnixFileMode)493); File.SetUnixFileMode(output, (UnixFileMode)511);
        File.SetUnixFileMode(context, (UnixFileMode)420); File.SetUnixFileMode(Path.Combine(source, "unchanged"), (UnixFileMode)420);
        await File.WriteAllTextAsync(Path.Combine(directory, "Dockerfile"), "FROM node:24-bookworm-slim\nCOPY probe.mjs /probe.mjs\nRUN chmod 644 /probe.mjs\nENTRYPOINT [\"node\",\"/probe.mjs\"]\n");
        await File.WriteAllTextAsync(Path.Combine(directory, "probe.mjs"), """
            import fs from 'node:fs';
            if (process.argv[2] === 'cleanup-session') process.exit(0);
            if (JSON.parse(fs.readFileSync('/input/context.json')).wait) await new Promise(r => setTimeout(r, 60000));
            let readOnly = false;
            try { fs.writeFileSync('/workspace/unchanged','modified'); } catch { readOnly = true; }
            fs.writeFileSync('/output/envelope.json', JSON.stringify({readOnly,githubCredential:!!process.env.GH_TOKEN || !!process.env.GITHUB_TOKEN,dockerSocket:fs.existsSync('/var/run/docker.sock')}));
            """);
        using var limit = new CancellationTokenSource(TimeSpan.FromMinutes(2));
        await ProcessRunner.Run("docker", ["build", "-t", image, directory], limit.Token);
        engine = new(new RuntimeOptions { Socket = "unused", ClientUid = 1, PolicyFile = "unused", ContractRoot = "unused", WorkRoot = directory, GithubTokenFile = "unused", EngineImage = image, EngineAuthVolume = volume, Model = "unused" });
    }
    Run Run() => new(Guid.NewGuid().ToString("N"), "owner/repo", 1, "{}", "running", 1, DateTime.UtcNow.AddMinutes(2), null, null, null, null);
    [Fact]
    public async Task EngineCannotWriteSourceOrSeePublisherCredentialOrDockerSocket()
    {
        using var limit = new CancellationTokenSource(TimeSpan.FromSeconds(30));
        var value = await engine.Execute(Run(), source, output, context, limit.Token);
        Assert.Equal("{\"readOnly\":true,\"githubCredential\":false,\"dockerSocket\":false}", value);
        Assert.Equal("original", await File.ReadAllTextAsync(Path.Combine(source, "unchanged")));
    }
    [Fact]
    public async Task CancellationRemovesTheRunningContainer()
    {
        await File.WriteAllTextAsync(context, "{\"wait\":true}");
        var run = Run();
        using var cancellation = new CancellationTokenSource(TimeSpan.FromSeconds(20));
        var execution = engine.Execute(run, source, output, context, cancellation.Token);
        using var poll = new CancellationTokenSource(TimeSpan.FromSeconds(10));
        while (!(await ProcessRunner.Run("docker", ["ps", "--filter", $"name=^{Engine.ContainerName(run.Id)}$", "--format", "{{.Names}}"], poll.Token)).Contains(Engine.ContainerName(run.Id))) await Task.Delay(100, poll.Token);
        cancellation.Cancel();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => execution);
        Assert.Empty((await ProcessRunner.Run("docker", ["ps", "-a", "--filter", $"name=^{Engine.ContainerName(run.Id)}$", "--format", "{{.Names}}"], CancellationToken.None)).Trim());
    }
    public async Task DisposeAsync()
    {
        await ProcessRunner.Run("docker", ["image", "rm", image], CancellationToken.None);
        await ProcessRunner.Run("docker", ["volume", "rm", volume], CancellationToken.None);
        Directory.Delete(directory, true);
    }
}
