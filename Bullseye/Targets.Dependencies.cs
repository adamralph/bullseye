namespace Bullseye
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    public static partial class Targets
    {
        public static ITarget Target(string name, IEnumerable<ITarget> dependsOn, Func<Task> action) => Target(name, dependsOn?.Select(target => target.Name), action);

        public static ITarget Target<TInput>(string name, IEnumerable<ITarget> dependsOn, IEnumerable<TInput> forEach, Func<TInput, Task> action) => Target(name, dependsOn?.Select(target => target.Name), forEach, action);

        public static ITarget Target(string name, IEnumerable<ITarget> dependsOn, Action action) => Target(name, dependsOn?.Select(target => target.Name), action);

        public static ITarget Target<TInput>(string name, IEnumerable<ITarget> dependsOn, IEnumerable<TInput> forEach, Action<TInput> action) => Target(name, dependsOn?.Select(target => target.Name), forEach, action);
    }
}
