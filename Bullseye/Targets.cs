namespace Bullseye
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Bullseye.Internal;

    public static partial class Targets
    {
        private static readonly TargetCollection targets = new TargetCollection();

        public static string[] DependsOn(params string[] dependencies) => dependencies;

        public static TInput[] ForEach<TInput>(params TInput[] inputs) => inputs;

        public static void Target(string name, IEnumerable<string> dependsOn) =>
            targets.Add(new Target(name, dependsOn));

        public static void Target(string name, IEnumerable<string> dependsOn, Func<Task> action) =>
            targets.Add(new ActionTarget(name, dependsOn, action));

        public static void Target<TInput>(string name, IEnumerable<string> dependsOn, IEnumerable<TInput> forEach, Func<TInput, Task> action) =>
            targets.Add(new ActionTarget<TInput>(name, dependsOn, forEach, action));

        [Obsolete("Use RunTargetsAndExitAsync instead. This method will be removed in 3.0.0.")]
        public static Task RunTargetsAsync(IEnumerable<string> args) =>
            targets.RunAsync(args);

        /// <summary>
        /// Runs the previously specified targets and then calls <see cref="Environment.Exit(int)"/>.
        /// Any code which follows a call to this method will not be executed.
        /// </summary>
        /// <param name="args">The command line arguments.</param>
        /// <param name="messageOnly">
        /// A predicate that is called when an exception is thrown.
        /// Return <c>true</c> to display only the exception message instead instead of the full exception details.
        /// </param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous running of the targets.</returns>
        public static Task RunTargetsAndExitAsync(IEnumerable<string> args, Func<Exception, bool> messageOnly) =>
            targets.RunAndExitAsync(args, messageOnly);

        /// <summary>
        /// Runs the previously specified targets and then calls <see cref="Environment.Exit(int)"/>.
        /// Any code which follows a call to this method will not be executed.
        /// </summary>
        /// <param name="args">The command line arguments.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous running of the targets.</returns>
        public static Task RunTargetsAndExitAsync(IEnumerable<string> args) =>
            RunTargetsAndExitAsync(args, default);
    }
}
