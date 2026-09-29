using System.Runtime.InteropServices;

namespace Bullseye.Internal;

public partial class Output
{
    public async Task<IAsyncDisposable> Initialize()
    {
        if (noColor || osPlatform != OSPlatform.Windows)
        {
            return new NullAsyncDisposable();
        }

        var diagnostics = Verbose ? diagnosticsWriter : TextWriter.Null;
        var prefix = Verbose ? getPrefix : () => "";

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

        var newMode = oldMode | NativeMethods.ConsoleOutputModes.EnableVirtualTerminalProcessing;

        await NativeMethodsWrapper.TrySetConsoleScreenBufferOutputMode(handle, newMode, diagnostics, prefix).Tax();

        return new State(handle, oldMode, diagnostics, prefix);
    }

    private sealed class State(
        IntPtr handle,
        NativeMethods.ConsoleOutputModes oldMode,
        TextWriter diagnostics,
        Func<string> getMessagePrefix)
        : IAsyncDisposable
    {
        public Task DisposeAsync() =>
            NativeMethodsWrapper.TrySetConsoleScreenBufferOutputMode(handle, oldMode, diagnostics, getMessagePrefix);
    }

    private sealed class NullAsyncDisposable : IAsyncDisposable
    {
        public Task DisposeAsync() => Task.CompletedTask;
    }
}
