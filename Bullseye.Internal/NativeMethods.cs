namespace Bullseye.Internal
{
    using System;
    using System.Runtime.InteropServices;

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

        public enum StdHandle
        {
            STD_OUTPUT_HANDLE = -11
        }
#pragma warning restore CA1707 // Identifiers should not contain underscores

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern IntPtr GetStdHandle(StdHandle nStdHandle);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool GetConsoleMode(IntPtr hConsoleHandle, out ConsoleOutputModes lpMode);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool SetConsoleMode(IntPtr hConsoleHandle, ConsoleOutputModes dwMode);
    }
}
