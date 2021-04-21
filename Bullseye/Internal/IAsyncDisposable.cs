using System.Threading.Tasks;

namespace Bullseye.Internal
{
    public interface IAsyncDisposable
    {
        Task DisposeAsync();
    }
}
