using SoFa.Runtime;

namespace SoFa.Runtime.Tests;

public sealed class ProcessTests
{
    [Fact]
    public async Task CancellationStopsTheProcessTree()
    {
        using var timeout = new CancellationTokenSource(TimeSpan.FromMilliseconds(200));
        var started = DateTime.UtcNow;
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => ProcessRunner.Run("sh", ["-c", "sleep 60 & wait"], timeout.Token));
        Assert.True(DateTime.UtcNow - started < TimeSpan.FromSeconds(5));
    }
    [Fact]
    public async Task OutputCannotGrowWithoutBound()
    {
        await Assert.ThrowsAnyAsync<Exception>(() => ProcessRunner.Run("sh", ["-c", "yes output"], CancellationToken.None, maxOutput: 8192));
    }
    [Fact]
    public async Task ArgumentsAreNotShellExpanded()
    {
        Assert.Equal("$(echo unsafe)", await ProcessRunner.Run("printf", ["%s", "$(echo unsafe)"], CancellationToken.None));
    }
}
