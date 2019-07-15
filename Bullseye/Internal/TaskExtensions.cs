namespace Bullseye.Internal
{
    using System.Runtime.CompilerServices;
    using System.Threading.Tasks;

    internal static class TaskExtensions
    {
        public static ConfiguredTaskAwaitable Tax(this Task task) => task.ConfigureAwait(false);
    }
}
