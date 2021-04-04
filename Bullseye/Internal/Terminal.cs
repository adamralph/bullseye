using System;
using System.IO;
using System.Threading.Tasks;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
namespace Bullseye.Internal
{
    public static class Terminal
    {
        public static async Task<IAsyncDisposable> TryConfigure(bool noColor, OperatingSystem operatingSystem, TextWriter log, string logPrefix)
        {
            if (noColor || operatingSystem != OperatingSystem.Windows)
            {
                return new NullAsyncDisposable();
            }

            var (handle, gotHandle) = await NativeMethodsWrapper.TryGetStandardOutputHandle(log, logPrefix).Tax();
            if (!gotHandle)
            {
                return new NullAsyncDisposable();
            }

            var (oldMode, gotMode) = await NativeMethodsWrapper.TryGetConsoleScreenBufferOutputMode(handle, log, logPrefix).Tax();
            if (!gotMode)
            {
                return new NullAsyncDisposable();
            }

            var newMode = oldMode | NativeMethods.ConsoleOutputModes.ENABLE_VIRTUAL_TERMINAL_PROCESSING;

            await NativeMethodsWrapper.TrySetConsoleScreenBufferOutputMode(handle, newMode, log, logPrefix).Tax();

            return new State(handle, oldMode, log, logPrefix);
        }

        private class State : IAsyncDisposable
        {
            private readonly IntPtr handle;
            private readonly NativeMethods.ConsoleOutputModes oldMode;
            private readonly TextWriter log;
            private readonly string logPrefix;

            public State(IntPtr handle, NativeMethods.ConsoleOutputModes oldMode, TextWriter log, string logPrefix) =>
                (this.handle, this.oldMode, this.log, this.logPrefix) = (handle, oldMode, log, logPrefix);

            public Task DisposeAsync() =>
                NativeMethodsWrapper.TrySetConsoleScreenBufferOutputMode(this.handle, this.oldMode, this.log, this.logPrefix);
        }

        private class NullAsyncDisposable : IAsyncDisposable
        {
            public Task DisposeAsync() => Task.CompletedTask;
        }
    }
}
