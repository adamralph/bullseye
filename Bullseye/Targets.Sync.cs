namespace Bullseye
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public static partial class Targets
    {
        public static void Target(string name, IEnumerable<string> dependsOn, Action action) =>
            Target(
                name,
                dependsOn,
                action == null
                    ? default(Func<Task>)
                    : () => Task.Run(action.Invoke));

        public static void Target<TInput>(string name, IEnumerable<string> dependsOn, IEnumerable<TInput> forEach, Action<TInput> action) =>
            Target(
                name,
                dependsOn,
                forEach,
                action == null
                    ? default(Func<TInput, Task>)
                    : input => Task.Run(() => action.Invoke(input)));

        [Obsolete("This method will be removed in 3.0.0. Consider switching to RunTargetsAndExit, which represents the canonical usage. If you really need to continue code execution after running the targets, use RunTargetsWithoutExiting instead.")]
        public static void RunTargets(IEnumerable<string> args) => RunTargetsWithoutExiting(args);

        /// <summary>
        /// Runs the previously specified targets.
        /// In most cases, <see cref="RunTargetsAndExit(IEnumerable{string}, Func{Exception, bool})"/> should be used instead of this method.
        /// This method should only be used if continued code execution after running targets is specifically required.
        /// </summary>
        /// <param name="args">The command line arguments.</param>
        /// <param name="messageOnly">
        /// A predicate that is called when an exception is thrown.
        /// Return <c>true</c> to display only the exception message instead instead of the full exception details.
        /// </param>
        public static void RunTargetsWithoutExiting(IEnumerable<string> args, Func<Exception, bool> messageOnly) =>
            RunTargetsWithoutExitingAsync(args, messageOnly).GetAwaiter().GetResult();

        /// <summary>
        /// Runs the previously specified targets.
        /// In most cases, <see cref="RunTargetsAndExit(IEnumerable{string})"/> should be used instead of this method.
        /// This method should only be used if continued code execution after running targets is specifically required.
        /// </summary>
        /// <param name="args">The command line arguments.</param>
        public static void RunTargetsWithoutExiting(IEnumerable<string> args) =>
            RunTargetsWithoutExitingAsync(args).GetAwaiter().GetResult();

        /// <summary>
        /// Runs the previously specified targets and then calls <see cref="Environment.Exit(int)"/>.
        /// Any code which follows a call to this method will not be executed.
        /// </summary>
        /// <param name="args">The command line arguments.</param>
        /// <param name="messageOnly">
        /// A predicate that is called when an exception is thrown.
        /// Return <c>true</c> to display only the exception message instead instead of the full exception details.
        /// </param>
        public static void RunTargetsAndExit(IEnumerable<string> args, Func<Exception, bool> messageOnly) =>
            RunTargetsAndExitAsync(args, messageOnly).GetAwaiter().GetResult();

        /// <summary>
        /// Runs the previously specified targets and then calls <see cref="Environment.Exit(int)"/>.
        /// Any code which follows a call to this method will not be executed.
        /// </summary>
        /// <param name="args">The command line arguments.</param>
        public static void RunTargetsAndExit(IEnumerable<string> args) =>
            RunTargetsAndExitAsync(args).GetAwaiter().GetResult();
    }
}
