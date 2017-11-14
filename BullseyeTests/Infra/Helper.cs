namespace BullseyeTests.Infra
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Bullseye.Internal;

    internal static class Helper
    {
        public static ref T Ensure<T>(ref T t) where T : class, new()
        {
            t = t ?? new T();
            return ref t;
        }

        public static Target CreateTarget(Action action) => CreateTarget(new string[0], action);

        public static Target CreateTarget(string[] dependencies, Action action) =>
            new Target(dependencies.ToList(), action.ToAsync());

        private static Func<Task> ToAsync(this Action action) =>
            () =>
            {
                action();
                return Task.FromResult(0);
            };
    }
}
