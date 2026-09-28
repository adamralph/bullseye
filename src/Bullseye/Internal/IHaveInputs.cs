namespace Bullseye.Internal;

public interface IHaveInputs
{
    IEnumerable<object?> Inputs { get; }
}
