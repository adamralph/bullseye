#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
namespace Bullseye.Internal
{
    using System.Collections.Generic;

    interface IHaveInputs
    {
        IEnumerable<object> Inputs { get; }
    }
}
