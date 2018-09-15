namespace Bullseye.Internal
{
    using System.Collections.Generic;

    interface IHaveInputs
    {
        IEnumerable<object> Inputs { get; }
    }
}
