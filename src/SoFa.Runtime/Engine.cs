using System.Text.Json.Nodes;

namespace SoFa.Runtime;

public sealed class Engine(RuntimeOptions options)
{
    public static string ContainerName(string id) => $"sofa-stage-{id}";
    public async Task Stop(string id, CancellationToken token)
    {
        var name = ContainerName(id);
        var found = await ProcessRunner.Run("docker", ["ps", "-a", "--filter", $"name=^{name}$", "--format", "{{.Names}}"], token);
        if (found.Trim() == name) await ProcessRunner.Run("docker", ["rm", "-f", name], token);
        await ProcessRunner.Run("docker", ["run", "--rm", "--network", "none", "--user", $"{options.EngineUid}:{options.EngineUid}",
            "--cpus", "1", "--memory", "512m", "--pids-limit", "64", "--cap-drop", "ALL", "--security-opt", "no-new-privileges", "--read-only",
            "--tmpfs", "/tmp:rw,size=64m,mode=1777", "--tmpfs", $"/home/engine/.cache:rw,size=64m,uid={options.EngineUid},gid={options.EngineUid}",
            "--tmpfs", $"/home/engine/.config:rw,size=16m,uid={options.EngineUid},gid={options.EngineUid}",
            "--tmpfs", $"/home/engine/.local/state:rw,size=64m,uid={options.EngineUid},gid={options.EngineUid}",
            "--mount", $"type=volume,src={options.EngineAuthVolume},dst=/home/engine/.local/share/opencode",
            options.EngineImage, "cleanup-session", $"ses_{id}"], token, maxOutput: 65536);
    }
    public async Task<string> Execute(Run run, string source, string output, string context, CancellationToken token)
    {
        var args = new[] { "run", "--rm", "--name", ContainerName(run.Id), "--user", $"{options.EngineUid}:{options.EngineUid}",
            "--cpus", "2", "--memory", "2g", "--pids-limit", "128", "--cap-drop", "ALL", "--security-opt", "no-new-privileges", "--read-only",
            "--tmpfs", "/tmp:rw,size=256m,mode=1777", "--tmpfs", $"/home/engine/.cache:rw,size=256m,uid={options.EngineUid},gid={options.EngineUid}",
            "--tmpfs", $"/home/engine/.config:rw,size=16m,uid={options.EngineUid},gid={options.EngineUid}",
            "--tmpfs", $"/home/engine/.local/state:rw,size=64m,uid={options.EngineUid},gid={options.EngineUid}",
            "--mount", $"type=bind,src={source},dst=/workspace,readonly", "--mount", $"type=bind,src={output},dst=/output",
            "--mount", $"type=bind,src={context},dst=/input/context.json,readonly", "--mount", $"type=volume,src={options.EngineAuthVolume},dst=/home/engine/.local/share/opencode",
            "--env", $"SOFA_SESSION=ses_{run.Id}", "--env", $"SOFA_MODEL={options.Model}", options.EngineImage };
        try { await ProcessRunner.Run("docker", args, token, maxOutput: 65536); }
        finally
        {
            using var cleanup = new CancellationTokenSource(TimeSpan.FromSeconds(30));
            await Stop(run.Id, cleanup.Token);
        }
        var result = new FileInfo(Path.Combine(output, "envelope.json"));
        if (!result.Exists || result.LinkTarget is not null || result.Length > 128_000) throw new Rejected("Engine did not produce a bounded regular result file");
        return await File.ReadAllTextAsync(result.FullName, token);
    }
}
