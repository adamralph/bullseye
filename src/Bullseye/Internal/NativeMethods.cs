using System.Runtime.InteropServices;

namespace Bullseye.Internal;

internal static partial class NativeMethods
{
    [Flags]
    public enum ConsoleOutputModes : uint
    {
        // ENABLE_VIRTUAL_TERMINAL_PROCESSING
        EnableVirtualTerminalProcessing = 0x0004,
    }

    public enum StdHandle
    {
        // STD_OUTPUT_HANDLE
        StdOutputHandle = -11,
    }

    [LibraryImport("kernel32.dll", SetLastError = true)]
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    public static partial nint GetStdHandle(StdHandle nStdHandle);

    [LibraryImport("kernel32.dll", SetLastError = true)]
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool GetConsoleMode(nint hConsoleHandle, out ConsoleOutputModes lpMode);

    [LibraryImport("kernel32.dll", SetLastError = true)]
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool SetConsoleMode(nint hConsoleHandle, ConsoleOutputModes dwMode);
}
