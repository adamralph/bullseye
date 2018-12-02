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

        [Obsolete("Use RunTargetsAndExitAsync(IEnumerable<string> args) instead. This method will be removed in version 3.0.0.")]
        public static Task RunTargetsAsync(IEnumerable<string> args) =>
            targets.RunAsync(args, new SystemConsole());

        /// <summary>
        /// Runs the previously specified targets and then calls <see cref="Environment.Exit(int)"/>.
        /// </summary>
        /// <param name="args">The command line arguments.</param>
        /// <param name="exceptionMessageOnly">The exception types for which to log the message only.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous running of the targets.</returns>
        public static Task RunTargetsAndExitAsync(IEnumerable<string> args, IEnumerable<Type> exceptionMessageOnly) =>
            targets.RunAndExitAsync(args, exceptionMessageOnly);

        /// <summary>
        /// Runs the previously specified targets and then calls <see cref="Environment.Exit(int)"/>.
        /// </summary>
        /// <param name="args">The command line arguments.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous running of the targets.</returns>
        public static Task RunTargetsAndExitAsync(IEnumerable<string> args) =>
            RunTargetsAndExitAsync(args, default);

        /// <summary>
        /// Runs the previously specified targets and then calls <see cref="Environment.Exit(int)"/>.
        /// </summary>
        /// <typeparam name="T">The exception type for which to log the message only.</typeparam>
        /// <param name="args">The command line arguments.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous running of the targets.</returns>
        public static Task RunTargetsAndExitAsync<T>(IEnumerable<string> args)
            where T : Exception
            => RunTargetsAndExitAsync(args, new[] { typeof(T) });

        /// <summary>
        /// Runs the previously specified targets and then calls <see cref="Environment.Exit(int)"/>.
        /// </summary>
        /// <typeparam name="T1">An exception type for which to log the message only.</typeparam>
        /// <typeparam name="T2">An exception type for which to log the message only.</typeparam>
        /// <param name="args">The command line arguments.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous running of the targets.</returns>
        public static Task RunTargetsAndExitAsync<T1, T2>(IEnumerable<string> args)
            where T1 : Exception where T2 : Exception
            => RunTargetsAndExitAsync(args, new[] { typeof(T1), typeof(T2) });

        /// <summary>
        /// Runs the previously specified targets and then calls <see cref="Environment.Exit(int)"/>.
        /// </summary>
        /// <typeparam name="T1">An exception type for which to log the message only.</typeparam>
        /// <typeparam name="T2">An exception type for which to log the message only.</typeparam>
        /// <typeparam name="T3">An exception type for which to log the message only.</typeparam>
        /// <param name="args">The command line arguments.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous running of the targets.</returns>
        public static Task RunTargetsAndExitAsync<T1, T2, T3>(IEnumerable<string> args)
            where T1 : Exception where T2 : Exception where T3 : Exception
            => RunTargetsAndExitAsync(args, new[] { typeof(T1), typeof(T2), typeof(T3) });

        /// <summary>
        /// Runs the previously specified targets and then calls <see cref="Environment.Exit(int)"/>.
        /// </summary>
        /// <typeparam name="T1">An exception type for which to log the message only.</typeparam>
        /// <typeparam name="T2">An exception type for which to log the message only.</typeparam>
        /// <typeparam name="T3">An exception type for which to log the message only.</typeparam>
        /// <typeparam name="T4">An exception type for which to log the message only.</typeparam>
        /// <param name="args">The command line arguments.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous running of the targets.</returns>
        public static Task RunTargetsAndExitAsync<T1, T2, T3, T4>(IEnumerable<string> args)
            where T1 : Exception where T2 : Exception where T3 : Exception where T4 : Exception
            => RunTargetsAndExitAsync(args, new[] { typeof(T1), typeof(T2), typeof(T3), typeof(T4) });
    }
}
