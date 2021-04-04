using System.Threading.Tasks;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
namespace Bullseye.Internal
{
    public interface IAsyncDisposable
    {
        Task DisposeAsync();
    }
}
