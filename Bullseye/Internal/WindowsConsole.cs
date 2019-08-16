namespace Bullseye.Internal
{
    using System.IO;
    using System.Threading.Tasks;

    public static class WindowsConsole
    {
        public static async Task TryEnableVirtualTerminalProcessing(TextWriter log, bool verbose)
        {
            (var handle, var gotHandle) = await NativeMethodsWrapper.TryGetStandardOutputHandle(log).Tax();
            if (!gotHandle)
            {
                return;
            }

            (var mode, var gotMode) = await NativeMethodsWrapper.TryGetConsoleScreenBufferOutputMode(handle, verbose ? log : NullTextWriter.Instance).Tax();
            if (!gotMode)
            {
                return;
            }

            mode |= NativeMethods.ConsoleOutputModes.ENABLE_VIRTUAL_TERMINAL_PROCESSING;

            await NativeMethodsWrapper.TrySetConsoleScreenBufferOutputMode(handle, mode, verbose ? log : NullTextWriter.Instance).Tax();
        }
    }
}
