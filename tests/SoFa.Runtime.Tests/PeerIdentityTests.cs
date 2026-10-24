using System.Net.Sockets;
using System.Runtime.InteropServices;
using SoFa.Runtime;

namespace SoFa.Runtime.Tests;

public sealed class PeerIdentityTests
{
    [DllImport("libc")] static extern uint getuid();

    [Fact]
    public async Task UnixPeerUsesKernelIdentityAndRejectsDifferentUid()
    {
        var path = Path.Combine(Path.GetTempPath(), $"sofa-peer-{Guid.NewGuid():N}.sock");
        try
        {
            using var server = new Socket(AddressFamily.Unix, SocketType.Stream, ProtocolType.Unspecified);
            server.Bind(new UnixDomainSocketEndPoint(path)); server.Listen(1);
            using var client = new Socket(AddressFamily.Unix, SocketType.Stream, ProtocolType.Unspecified);
            await client.ConnectAsync(new UnixDomainSocketEndPoint(path));
            using var accepted = await server.AcceptAsync();
            Assert.True(PeerIdentity.IsAuthorized(accepted, getuid()));
            Assert.False(PeerIdentity.IsAuthorized(accepted, getuid() + 1));
        }
        finally { File.Delete(path); }
    }
}
