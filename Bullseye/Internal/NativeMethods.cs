using System;
using System.Runtime.InteropServices;

namespace Bullseye.Internal
{
    internal static class NativeMethods
    {
        [Flags]
        public enum ConsoleOutputModes : uint
        {
            ENABLE_VIRTUAL_TERMINAL_PROCESSING = 0x0004,
        }

        public enum StdHandle
        {
            STD_OUTPUT_HANDLE = -11
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        public static extern IntPtr GetStdHandle(StdHandle nStdHandle);

        [DllImport("kernel32.dll", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        public static extern bool GetConsoleMode(IntPtr hConsoleHandle, out ConsoleOutputModes lpMode);

        [DllImport("kernel32.dll", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        public static extern bool SetConsoleMode(IntPtr hConsoleHandle, ConsoleOutputModes dwMode);
    }
}
