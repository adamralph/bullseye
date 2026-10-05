namespace Bullseye.Internal;

internal interface IHaveInputs
{
    IEnumerable<object?> Inputs { get; }
}
