#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
namespace Bullseye.Internal
{
    using System;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Threading.Tasks;

    public static class NativeMethodsWrapper
    {
        public static async Task<(IntPtr handle, bool succeeded)> TryGetStandardOutputHandle(TextWriter log, string logPrefix)
        {
            var (handle, error) = (NativeMethods.GetStdHandle(NativeMethods.StdHandle.STD_OUTPUT_HANDLE), Marshal.GetLastWin32Error());

            if (error != 0)
            {
                await log.WriteLineAsync($"{logPrefix}: Failed to get a handle to the standard output device (GetStdHandle). Error code: {error}").Tax();
                return default;
            }

            await log.WriteLineAsync($"{logPrefix}: Got a handle to the standard output device (GetStdHandle): {handle}").Tax();
            return (handle, true);
        }

        public static async Task<(NativeMethods.ConsoleOutputModes mode, bool succeeded)> TryGetConsoleScreenBufferOutputMode(IntPtr standardOutputHandle, TextWriter log, string logPrefix)
        {
            if (!NativeMethods.GetConsoleMode(standardOutputHandle, out var mode))
            {
                await log.WriteLineAsync($"{logPrefix}: Failed to get the current output mode of the console screen buffer (GetConsoleMode). Error code: {Marshal.GetLastWin32Error()}").Tax();
                return default;
            }

            await log.WriteLineAsync($"{logPrefix}: Got the current output mode of the console screen buffer (GetConsoleMode): {mode}").Tax();
            return (mode, true);
        }

        public static async Task TrySetConsoleScreenBufferOutputMode(IntPtr standardOutputHandle, NativeMethods.ConsoleOutputModes mode, TextWriter log, string logPrefix)
        {
            if (!NativeMethods.SetConsoleMode(standardOutputHandle, mode))
            {
                await log.WriteLineAsync($"{logPrefix}: Failed to set the output mode of the console screen buffer (SetConsoleMode). Error code: {Marshal.GetLastWin32Error()}").Tax();
            }

            await log.WriteLineAsync($"{logPrefix}: Set the current output mode of the console screen buffer (SetConsoleMode): {mode}").Tax();
        }
    }
}
