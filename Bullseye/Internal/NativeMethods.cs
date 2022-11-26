using System;
using System.Runtime.InteropServices;

namespace Bullseye.Internal
{
#if NET6_0
    internal static class NativeMethods
#else
    internal static partial class NativeMethods
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

#if NET6_0
        [DllImport("kernel32.dll", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        public static extern IntPtr GetStdHandle(StdHandle nStdHandle);
#else
        [LibraryImport("kernel32.dll", SetLastError = true)]
        public static partial nint GetStdHandle(StdHandle nStdHandle);
#endif

#if NET6_0
        [DllImport("kernel32.dll", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        public static extern bool GetConsoleMode(nint hConsoleHandle, out ConsoleOutputModes lpMode);
#else
        [LibraryImport("kernel32.dll", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static partial bool GetConsoleMode(nint hConsoleHandle, out ConsoleOutputModes lpMode);
#endif

#if NET6_0
        [DllImport("kernel32.dll", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        public static extern bool SetConsoleMode(nint hConsoleHandle, ConsoleOutputModes dwMode);
#else
        [LibraryImport("kernel32.dll", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static partial bool SetConsoleMode(nint hConsoleHandle, ConsoleOutputModes dwMode);
#endif
    }
}
