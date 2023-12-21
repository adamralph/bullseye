using Bullseye.Internal;

namespace Bullseye
{
    /// <summary>
    /// Provides methods for defining and running targets.
    /// </summary>
    public partial class Targets
    {
#if NET8_0_OR_GREATER
        private readonly TargetCollection targetCollection = [];
#else
        private readonly TargetCollection targetCollection = new();
#endif
    }
}
