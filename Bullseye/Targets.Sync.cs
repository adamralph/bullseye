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
                    : () => Task.Run(() => action.Invoke()));

        public static void Target<TInput>(string name, IEnumerable<string> dependsOn, IEnumerable<TInput> forEach, Action<TInput> action) =>
            Target(
                name,
                dependsOn,
                forEach,
                action == null
                    ? default(Func<TInput, Task>)
                    : input => Task.Run(() => action.Invoke(input)));

        [Obsolete("Use RunTargetsAndExit(IEnumerable<string> args) instead. This method will be removed in version 3.0.0.")]
        public static void RunTargets(IEnumerable<string> args) => RunTargetsAsync(args).GetAwaiter().GetResult();

        /// <summary>
        /// Runs the previously specified targets and then calls <see cref="Environment.Exit(int)"/>.
        /// </summary>
        /// <param name="args">The command line arguments.</param>
        /// <param name="exceptionMessageOnly">The exception types for which to log the message only.</param>
        public static void RunTargetsAndExit(IEnumerable<string> args, IEnumerable<Type> exceptionMessageOnly) =>
            RunTargetsAndExitAsync(args, exceptionMessageOnly).GetAwaiter().GetResult();

        /// <summary>
        /// Runs the previously specified targets and then calls <see cref="Environment.Exit(int)"/>.
        /// </summary>
        /// <param name="args">The command line arguments.</param>
        public static void RunTargetsAndExit(IEnumerable<string> args) =>
            RunTargetsAndExitAsync(args).GetAwaiter().GetResult();

        /// <summary>
        /// Runs the previously specified targets and then calls <see cref="Environment.Exit(int)"/>.
        /// </summary>
        /// <typeparam name="T">The exception type for which to log the message only.</typeparam>
        /// <param name="args">The command line arguments.</param>
        public static void RunTargetsAndExit<T>(IEnumerable<string> args)
            where T : Exception
            => RunTargetsAndExitAsync<T>(args).GetAwaiter().GetResult();

        /// <summary>
        /// Runs the previously specified targets and then calls <see cref="Environment.Exit(int)"/>.
        /// </summary>
        /// <typeparam name="T1">An exception type for which to log the message only.</typeparam>
        /// <typeparam name="T2">An exception type for which to log the message only.</typeparam>
        /// <param name="args">The command line arguments.</param>
        public static void RunTargetsAndExit<T1, T2>(IEnumerable<string> args)
            where T1 : Exception where T2 : Exception
            => RunTargetsAndExitAsync<T1, T2>(args).GetAwaiter().GetResult();

        /// <summary>
        /// Runs the previously specified targets and then calls <see cref="Environment.Exit(int)"/>.
        /// </summary>
        /// <typeparam name="T1">An exception type for which to log the message only.</typeparam>
        /// <typeparam name="T2">An exception type for which to log the message only.</typeparam>
        /// <typeparam name="T3">An exception type for which to log the message only.</typeparam>
        /// <param name="args">The command line arguments.</param>
        public static void RunTargetsAndExit<T1, T2, T3>(IEnumerable<string> args)
            where T1 : Exception where T2 : Exception where T3 : Exception
            => RunTargetsAndExitAsync<T1, T2, T3>(args).GetAwaiter().GetResult();

        /// <summary>
        /// Runs the previously specified targets and then calls <see cref="Environment.Exit(int)"/>.
        /// </summary>
        /// <typeparam name="T1">An exception type for which to log the message only.</typeparam>
        /// <typeparam name="T2">An exception type for which to log the message only.</typeparam>
        /// <typeparam name="T3">An exception type for which to log the message only.</typeparam>
        /// <typeparam name="T4">An exception type for which to log the message only.</typeparam>
        /// <param name="args">The command line arguments.</param>
        public static void RunTargetsAndExit<T1, T2, T3, T4>(IEnumerable<string> args)
            where T1 : Exception where T2 : Exception where T3 : Exception where T4 : Exception
            => RunTargetsAndExitAsync<T1, T2, T3, T4>(args).GetAwaiter().GetResult();
    }
}
