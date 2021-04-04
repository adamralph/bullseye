using System;
using System.Runtime.InteropServices;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
namespace Bullseye.Internal
{
    public static class NativeMethods
    {
#pragma warning disable CA1707 // Identifiers should not contain underscores
        [Flags]
#pragma warning disable CA1028 // Enum Storage should be Int32
        public enum ConsoleOutputModes : uint
#pragma warning restore CA1028 // Enum Storage should be Int32
        {
            ENABLE_VIRTUAL_TERMINAL_PROCESSING = 0x0004,
        }

#pragma warning disable CA1008 // Enums should have zero value
        public enum StdHandle
#pragma warning restore CA1008 // Enums should have zero value
        {
            STD_OUTPUT_HANDLE = -11
        }
#pragma warning restore CA1707 // Identifiers should not contain underscores

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
