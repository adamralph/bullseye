#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
namespace Bullseye.Internal
{
    using System.IO;
    using System.Threading.Tasks;

    public static class Terminal
    {
        public static async Task TryConfigure(bool noColor, OperatingSystem operatingSystem, TextWriter log, string logPrefix)
        {
            if (noColor || operatingSystem != OperatingSystem.Windows)
            {
                return;
            }

            var (handle, gotHandle) = await NativeMethodsWrapper.TryGetStandardOutputHandle(log, logPrefix).Tax();
            if (!gotHandle)
            {
                return;
            }

            var (mode, gotMode) = await NativeMethodsWrapper.TryGetConsoleScreenBufferOutputMode(handle, log, logPrefix).Tax();
            if (!gotMode)
            {
                return;
            }

            mode |= NativeMethods.ConsoleOutputModes.ENABLE_VIRTUAL_TERMINAL_PROCESSING;

            await NativeMethodsWrapper.TrySetConsoleScreenBufferOutputMode(handle, mode, log, logPrefix).Tax();
        }
    }
}
