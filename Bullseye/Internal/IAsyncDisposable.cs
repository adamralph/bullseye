#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable RS0016 // Add public types and members to the declared API
namespace Bullseye.Internal
{
    using System.Threading.Tasks;

    public interface IAsyncDisposable
    {
        Task DisposeAsync();
    }
}
