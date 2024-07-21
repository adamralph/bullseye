using Bullseye.Internal;

namespace BullseyeTests.Infra;

internal static class Helper
{
    public static Target CreateTarget(string name, Action action) => CreateTarget(name, [], action);

    public static Target CreateTarget(string name, IEnumerable<string> dependencies, Action action) =>
        new ActionTarget(name, "", dependencies, action.ToAsync());

    public static Target CreateTarget(string name, IEnumerable<string> dependencies) =>
        new(name, "", dependencies);

    public static Target CreateTarget<TInput>(string name, IEnumerable<TInput> forEach, Action<TInput> action) =>
        new ActionTarget<TInput>(name, "", [], forEach, action.ToAsync());
}
