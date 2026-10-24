using System.Diagnostics;
using System.Text;

namespace SoFa.Runtime;

public sealed class ProcessFailure(string executable, int exitCode, string output)
    : Rejected($"{Path.GetFileName(executable)} failed with exit code {exitCode}")
{
    public int ExitCode { get; } = exitCode;
    public string Output { get; } = output;
}

public static class ProcessRunner
{
    public static async Task<string> Run(string executable, IEnumerable<string> args, CancellationToken cancellation, string? input = null, int maxOutput = 1_048_576)
    {
        var start = new ProcessStartInfo(executable) { RedirectStandardOutput = true, RedirectStandardError = true, RedirectStandardInput = true, UseShellExecute = false };
        foreach (var arg in args) start.ArgumentList.Add(arg);
        using var process = Process.Start(start) ?? throw new InvalidOperationException("Cannot start process");
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellation);
        using var registration = timeout.Token.Register(() => { try { process.Kill(entireProcessTree: true); } catch (InvalidOperationException) { } });
        async Task<string> Read(StreamReader reader)
        {
            var text = new StringBuilder();
            var buffer = new char[4096];
            int count;
            while ((count = await reader.ReadAsync(buffer, timeout.Token)) > 0)
            {
                if (text.Length + count > maxOutput) { timeout.Cancel(); throw new Rejected("Process output exceeded its bound"); }
                text.Append(buffer, 0, count);
            }
            return text.ToString();
        }
        var stdout = Read(process.StandardOutput);
        var stderr = Read(process.StandardError);
        if (input is not null) await process.StandardInput.WriteAsync(input.AsMemory(), timeout.Token);
        process.StandardInput.Close();
        await Task.WhenAll(stdout, stderr, process.WaitForExitAsync(timeout.Token));
        if (process.ExitCode != 0) throw new ProcessFailure(executable, process.ExitCode, await stdout);
        return await stdout;
    }
}
