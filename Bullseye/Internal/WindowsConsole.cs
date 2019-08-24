#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
namespace Bullseye.Internal
{
    using System.IO;
    using System.Threading.Tasks;

    public static class WindowsConsole
    {
        public static async Task TryEnableVirtualTerminalProcessing(TextWriter log, string logPrefix)
        {
            (var handle, var gotHandle) = await NativeMethodsWrapper.TryGetStandardOutputHandle(log, logPrefix).Tax();
            if (!gotHandle)
            {
                return;
            }

            (var mode, var gotMode) = await NativeMethodsWrapper.TryGetConsoleScreenBufferOutputMode(handle, log, logPrefix).Tax();
            if (!gotMode)
            {
                return;
            }

            mode |= NativeMethods.ConsoleOutputModes.ENABLE_VIRTUAL_TERMINAL_PROCESSING;

            await NativeMethodsWrapper.TrySetConsoleScreenBufferOutputMode(handle, mode, log, logPrefix).Tax();
        }
    }
}
