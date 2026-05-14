using System.Runtime.InteropServices;

namespace Bullseye.Internal;

internal static class NativeMethodsWrapper
{
    public static async Task<(IntPtr handle, bool succeeded)> TryGetStandardOutputHandle(Writer diagnostics, string messagePrefix)
    {
        var (handle, error) = (NativeMethods.GetStdHandle(NativeMethods.StdHandle.StdOutputHandle), Marshal.GetLastWin32Error());

        if (error != 0)
        {
            await diagnostics.VerboseAsync(() => $"{messagePrefix}: Failed to get a handle to the standard output device (GetStdHandle). Error code: {error}").Tax();
            return default;
        }

        await diagnostics.VerboseAsync(() => $"{messagePrefix}: Got a handle to the standard output device (GetStdHandle): {handle}").Tax();
        return (handle, true);
    }

    public static async Task<(NativeMethods.ConsoleOutputModes mode, bool succeeded)> TryGetConsoleScreenBufferOutputMode(IntPtr standardOutputHandle, Writer diagnostics, string messagePrefix)
    {
        if (!NativeMethods.GetConsoleMode(standardOutputHandle, out var mode))
        {
            await diagnostics.VerboseAsync(() => $"{messagePrefix}: Failed to get the current output mode of the console screen buffer (GetConsoleMode). Error code: {Marshal.GetLastWin32Error()}").Tax();
            return default;
        }

        await diagnostics.VerboseAsync(() => $"{messagePrefix}: Got the current output mode of the console screen buffer (GetConsoleMode): {mode}").Tax();
        return (mode, true);
    }

    public static async Task TrySetConsoleScreenBufferOutputMode(IntPtr standardOutputHandle, NativeMethods.ConsoleOutputModes mode, Writer diagnostics, string messagePrefix)
    {
        if (!NativeMethods.SetConsoleMode(standardOutputHandle, mode))
        {
            await diagnostics.VerboseAsync(() => $"{messagePrefix}: Failed to set the output mode of the console screen buffer (SetConsoleMode). Error code: {Marshal.GetLastWin32Error()}").Tax();
        }

        await diagnostics.VerboseAsync(() => $"{messagePrefix}: Set the current output mode of the console screen buffer (SetConsoleMode): {mode}").Tax();
    }
}
