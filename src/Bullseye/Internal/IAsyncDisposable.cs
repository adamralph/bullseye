namespace Bullseye.Internal;

public interface IAsyncDisposable
{
    Task DisposeAsync();
}
