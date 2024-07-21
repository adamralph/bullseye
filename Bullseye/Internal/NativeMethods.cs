using System.Runtime.InteropServices;

namespace Bullseye.Internal;

#if NET8_0_OR_GREATER
internal static partial class NativeMethods
#else
internal static class NativeMethods
#endif
{
    [Flags]
    public enum ConsoleOutputModes : uint
    {
        ENABLE_VIRTUAL_TERMINAL_PROCESSING = 0x0004,
    }

    public enum StdHandle
    {
        STD_OUTPUT_HANDLE = -11,
    }

#if NET8_0_OR_GREATER
    [LibraryImport("kernel32.dll", SetLastError = true)]
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    public static partial nint GetStdHandle(StdHandle nStdHandle);
#else
    [DllImport("kernel32.dll", SetLastError = true)]
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    public static extern IntPtr GetStdHandle(StdHandle nStdHandle);
#endif

#if NET8_0_OR_GREATER
    [LibraryImport("kernel32.dll", SetLastError = true)]
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool GetConsoleMode(nint hConsoleHandle, out ConsoleOutputModes lpMode);
#else
    [DllImport("kernel32.dll", SetLastError = true)]
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    public static extern bool GetConsoleMode(nint hConsoleHandle, out ConsoleOutputModes lpMode);
#endif

#if NET8_0_OR_GREATER
    [LibraryImport("kernel32.dll", SetLastError = true)]
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool SetConsoleMode(nint hConsoleHandle, ConsoleOutputModes dwMode);
#else
    [DllImport("kernel32.dll", SetLastError = true)]
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    public static extern bool SetConsoleMode(nint hConsoleHandle, ConsoleOutputModes dwMode);
#endif
}
