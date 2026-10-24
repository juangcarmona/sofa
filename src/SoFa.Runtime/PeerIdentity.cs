using System.Net.Sockets;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

namespace SoFa.Runtime;

public static class PeerIdentity
{
    [StructLayout(LayoutKind.Sequential)]
    struct Credentials { public int Process; public uint User; public uint Group; }

    // SafeHandle keeps the descriptor alive throughout the native call.
    [DllImport("libc", EntryPoint = "getsockopt", SetLastError = true)]
    static extern int GetCredentials(SafeSocketHandle socket, int level, int option, out Credentials credentials, ref uint size);

    public static bool IsAuthorized(Socket socket, uint expectedUser)
    {
        if (!OperatingSystem.IsLinux()) return false;
        uint size = (uint)Marshal.SizeOf<Credentials>();
        return GetCredentials(socket.SafeHandle, 1, 17, out var credentials, ref size) == 0 &&
            size == Marshal.SizeOf<Credentials>() && credentials.User == expectedUser;
    }
}
