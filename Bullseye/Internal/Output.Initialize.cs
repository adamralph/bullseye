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

        var prefix = getPrefix();

        var (handle, gotHandle) = await NativeMethodsWrapper.TryGetStandardOutputHandle(diagnosticsWriter, prefix).Tax();
        if (!gotHandle)
        {
            return new NullAsyncDisposable();
        }

        var (oldMode, gotMode) = await NativeMethodsWrapper.TryGetConsoleScreenBufferOutputMode(handle, diagnosticsWriter, prefix).Tax();
        if (!gotMode)
        {
            return new NullAsyncDisposable();
        }

        var newMode = oldMode | NativeMethods.ConsoleOutputModes.EnableVirtualTerminalProcessing;

        await NativeMethodsWrapper.TrySetConsoleScreenBufferOutputMode(handle, newMode, diagnosticsWriter, prefix).Tax();

        return new State(handle, oldMode, diagnosticsWriter, prefix);
    }

    private sealed class State(
        IntPtr handle,
        NativeMethods.ConsoleOutputModes oldMode,
        Writer diagnostics,
        string prefix)
        : IAsyncDisposable
    {
        public Task DisposeAsync() =>
            NativeMethodsWrapper.TrySetConsoleScreenBufferOutputMode(handle, oldMode, diagnostics, prefix);
    }

    private sealed class NullAsyncDisposable : IAsyncDisposable
    {
        public Task DisposeAsync() => Task.CompletedTask;
    }
}
