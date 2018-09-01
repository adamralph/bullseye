namespace Bullseye.Internal
{
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Threading.Tasks;

    public static class WindowsConsole
    {
        internal static async Task TryEnableVirtualTerminalProcessing(TextWriter @out, bool verbose)
        {
            const uint ENABLE_VIRTUAL_TERMINAL_PROCESSING = 0x0004;

            const int STD_OUTPUT_HANDLE = -11;

            var consoleHandle = NativeMethods.GetStdHandle(STD_OUTPUT_HANDLE);
            var lastError = Marshal.GetLastWin32Error();

            if (lastError != 0)
            {
                if (verbose)
                {
                    await @out.WriteLineAsync(
                        $"Bullseye: Failed to get a handle to the standard output device (GetStdHandle). Error code: {lastError}").ConfigureAwait(false);
                }

                return;
            }

            if (verbose)
            {
                await @out.WriteLineAsync($"Bullseye: Got a handle to the standard output device (GetStdHandle): {consoleHandle}").ConfigureAwait(false);
            }

            if (!NativeMethods.GetConsoleMode(consoleHandle, out var consoleMode))
            {
                if (verbose)
                {
                    await @out.WriteLineAsync(
                        $"Bullseye: Failed to retrieve the current output mode of the console screen buffer (GetConsoleMode). Error code: {Marshal.GetLastWin32Error()}").ConfigureAwait(false);
                }

                return;
            }

            if (verbose)
            {
                await @out.WriteLineAsync($"Bullseye: Retreived the current output mode of the console screen buffer (GetConsoleMode): {consoleMode}").ConfigureAwait(false);
            }

            consoleMode |= ENABLE_VIRTUAL_TERMINAL_PROCESSING;

            if (!NativeMethods.SetConsoleMode(consoleHandle, consoleMode))
            {
                if (verbose)
                {
                    await @out.WriteLineAsync(
                       $"Failed to set the output mode of the console screen buffer (SetConsoleMode). Error code: {Marshal.GetLastWin32Error()}").ConfigureAwait(false);
                }
            }

            if (verbose)
            {
                await @out.WriteLineAsync($"Bullseye: Set the current output mode of the console screen buffer (SetConsoleMode): {consoleMode}").ConfigureAwait(false);
            }
        }
    }
}
