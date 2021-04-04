using System.Collections.Generic;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
namespace Bullseye.Internal
{
    public interface IHaveInputs
    {
        IEnumerable<object> Inputs { get; }
    }
}
