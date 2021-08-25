using System;
using System.IO;
using System.Threading.Tasks;

namespace Bullseye.Internal
{
    public partial class Output
    {
        public async Task<IAsyncDisposable> Initialize(TextWriter diagnostics)
        {
            if (this.noColor || this.operatingSystem != OperatingSystem.Windows)
            {
                return new NullAsyncDisposable();
            }

            var (handle, gotHandle) = await NativeMethodsWrapper.TryGetStandardOutputHandle(diagnostics, this.prefix).Tax();
            if (!gotHandle)
            {
                return new NullAsyncDisposable();
            }

            var (oldMode, gotMode) = await NativeMethodsWrapper.TryGetConsoleScreenBufferOutputMode(handle, diagnostics, this.prefix).Tax();
            if (!gotMode)
            {
                return new NullAsyncDisposable();
            }

            var newMode = oldMode | NativeMethods.ConsoleOutputModes.ENABLE_VIRTUAL_TERMINAL_PROCESSING;

            await NativeMethodsWrapper.TrySetConsoleScreenBufferOutputMode(handle, newMode, diagnostics, this.prefix).Tax();

            return new State(handle, oldMode, diagnostics, this.prefix);
        }

        private class State : IAsyncDisposable
        {
            private readonly IntPtr handle;
            private readonly NativeMethods.ConsoleOutputModes oldMode;
            private readonly TextWriter diagnostics;
            private readonly string messagePrefix;

            public State(IntPtr handle, NativeMethods.ConsoleOutputModes oldMode, TextWriter diagnostics, string messagePrefix) =>
                (this.handle, this.oldMode, this.diagnostics, this.messagePrefix) = (handle, oldMode, diagnostics, messagePrefix);

            public Task DisposeAsync() =>
                NativeMethodsWrapper.TrySetConsoleScreenBufferOutputMode(this.handle, this.oldMode, this.diagnostics, this.messagePrefix);
        }

        private class NullAsyncDisposable : IAsyncDisposable
        {
            public Task DisposeAsync() => Task.CompletedTask;
        }
    }
}
