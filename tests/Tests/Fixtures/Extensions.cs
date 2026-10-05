using Bullseye.Internal;

namespace Tests.Fixtures;

internal static class Extensions
{
    extension(Target)
    {
        public static Target Create(string name, Action action) => Target.Create(name, [], action);

        public static Target Create(string name, IReadOnlyCollection<string> dependsOn, Action action) =>
            new ActionTarget(name, "", dependsOn, action.ToFunc());

        public static Target Create(string name, IReadOnlyCollection<string> dependsOn) =>
            new(name, "", dependsOn);

        public static Target Create<TInput>(string name, IEnumerable<TInput> forEach, Action<TInput> action) =>
            new ActionTarget<TInput>(name, "", [], forEach, action.ToFunc());
    }
}
