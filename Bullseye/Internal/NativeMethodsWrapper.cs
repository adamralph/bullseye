using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace Bullseye.Internal
{
    internal static class NativeMethodsWrapper
    {
        public static async Task<(IntPtr handle, bool succeeded)> TryGetStandardOutputHandle(TextWriter diagnostics, string messagePrefix)
        {
            diagnostics = diagnostics ?? Console.Error;

            var (handle, error) = (NativeMethods.GetStdHandle(NativeMethods.StdHandle.STD_OUTPUT_HANDLE), Marshal.GetLastWin32Error());

            if (error != 0)
            {
                await diagnostics.WriteLineAsync($"{messagePrefix}: Failed to get a handle to the standard output device (GetStdHandle). Error code: {error}").Tax();
                return default;
            }

            await diagnostics.WriteLineAsync($"{messagePrefix}: Got a handle to the standard output device (GetStdHandle): {handle}").Tax();
            return (handle, true);
        }

        public static async Task<(NativeMethods.ConsoleOutputModes mode, bool succeeded)> TryGetConsoleScreenBufferOutputMode(IntPtr standardOutputHandle, TextWriter diagnostics, string messagePrefix)
        {
            diagnostics = diagnostics ?? Console.Error;

            if (!NativeMethods.GetConsoleMode(standardOutputHandle, out var mode))
            {
                await diagnostics.WriteLineAsync($"{messagePrefix}: Failed to get the current output mode of the console screen buffer (GetConsoleMode). Error code: {Marshal.GetLastWin32Error()}").Tax();
                return default;
            }

            await diagnostics.WriteLineAsync($"{messagePrefix}: Got the current output mode of the console screen buffer (GetConsoleMode): {mode}").Tax();
            return (mode, true);
        }

        public static async Task TrySetConsoleScreenBufferOutputMode(IntPtr standardOutputHandle, NativeMethods.ConsoleOutputModes mode, TextWriter diagnostics, string messagePrefix)
        {
            diagnostics = diagnostics ?? Console.Error;

            if (!NativeMethods.SetConsoleMode(standardOutputHandle, mode))
            {
                await diagnostics.WriteLineAsync($"{messagePrefix}: Failed to set the output mode of the console screen buffer (SetConsoleMode). Error code: {Marshal.GetLastWin32Error()}").Tax();
            }

            await diagnostics.WriteLineAsync($"{messagePrefix}: Set the current output mode of the console screen buffer (SetConsoleMode): {mode}").Tax();
        }
    }
}
