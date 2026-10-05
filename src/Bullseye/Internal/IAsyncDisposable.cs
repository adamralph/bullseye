namespace Bullseye.Internal;

internal interface IAsyncDisposable
{
    Task DisposeAsync();
}
