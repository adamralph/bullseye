using System;
using System.IO;
using System.Threading.Tasks;

namespace Bullseye.Internal
{
    public partial class Output
    {
        public async Task<IAsyncDisposable> Initialize()
        {
            if (this.noColor || this.operatingSystem != OperatingSystem.Windows)
            {
                return new NullAsyncDisposable();
            }

            var diagnostics = this.Verbose ? this.diagnosticsWriter : TextWriter.Null;
            var prefix = this.Verbose ? this.getPrefix : () => "";

            var (handle, gotHandle) = await NativeMethodsWrapper.TryGetStandardOutputHandle(diagnostics, prefix).Tax();
            if (!gotHandle)
            {
                return new NullAsyncDisposable();
            }

            var (oldMode, gotMode) = await NativeMethodsWrapper.TryGetConsoleScreenBufferOutputMode(handle, diagnostics, prefix).Tax();
            if (!gotMode)
            {
                return new NullAsyncDisposable();
            }

            var newMode = oldMode | NativeMethods.ConsoleOutputModes.ENABLE_VIRTUAL_TERMINAL_PROCESSING;

            await NativeMethodsWrapper.TrySetConsoleScreenBufferOutputMode(handle, newMode, diagnostics, prefix).Tax();

            return new State(handle, oldMode, diagnostics, prefix);
        }

        private class State : IAsyncDisposable
        {
            private readonly IntPtr handle;
            private readonly NativeMethods.ConsoleOutputModes oldMode;
            private readonly TextWriter diagnostics;
            private readonly Func<string> getMessagePrefix;

            public State(IntPtr handle, NativeMethods.ConsoleOutputModes oldMode, TextWriter diagnostics, Func<string> getMessagePrefix) =>
                (this.handle, this.oldMode, this.diagnostics, this.getMessagePrefix) = (handle, oldMode, diagnostics, getMessagePrefix);

            public Task DisposeAsync() =>
                NativeMethodsWrapper.TrySetConsoleScreenBufferOutputMode(this.handle, this.oldMode, this.diagnostics, this.getMessagePrefix);
        }

        private class NullAsyncDisposable : IAsyncDisposable
        {
            public Task DisposeAsync() => Task.CompletedTask;
        }
    }
}
