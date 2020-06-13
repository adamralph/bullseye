namespace Bullseye.Internal
{
    using System.Collections.Generic;

    public interface IHaveInputs
    {
        IEnumerable<object> Inputs { get; }
    }
}
