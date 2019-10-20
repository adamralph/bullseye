namespace Bullseye
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Bullseye.Internal;

    public partial class Targets
    {
        /// <summary>
        /// Defines a target which depends on other targets and performs an action.
        /// </summary>
        /// <param name="name">The name of the target.</param>
        /// <param name="dependsOn">The names of the targets on which the target depends.</param>
        /// <param name="action">The action performed by the target.</param>
        public static void Target(string name, IEnumerable<string> dependsOn, Action action) =>
            Target(
                name,
                dependsOn,
                action == null
                    ? (Func<Task>)null
                    : () => Task.Run(action.Invoke));

        /// <summary>
        /// Defines a target which depends on other targets and performs an action for each item in a list of inputs.
        /// </summary>
        /// <typeparam name="TInput">The type of input required by <paramref name="action"/>.</typeparam>
        /// <param name="name">The name of the target.</param>
        /// <param name="dependsOn">The names of the targets on which the target depends.</param>
        /// <param name="forEach">The list of inputs to pass to <paramref name="action"/>.</param>
        /// <param name="action">The action performed by the target for each input in <paramref name="forEach"/>.</param>
        public static void Target<TInput>(string name, IEnumerable<string> dependsOn, IEnumerable<TInput> forEach, Action<TInput> action) =>
            Target(
                name,
                dependsOn,
                forEach,
                action == null
                    ? (Func<TInput, Task>)null
                    : input => Task.Run(() => action.Invoke(input)));

        /// <summary>
        /// Runs the previously specified targets.
        /// In most cases, <see cref="RunTargetsAndExit(IEnumerable{string}, Func{Exception, bool}, string)"/> should be used instead of this method.
        /// This method should only be used if continued code execution after running targets is specifically required.
        /// </summary>
        /// <param name="args">The command line arguments.</param>
        /// <param name="messageOnly">
        /// A predicate that is called when an exception is thrown.
        /// Return <c>true</c> to display only the exception message instead instead of the full exception details.
        /// </param>
        /// <param name="logPrefix">
        /// The prefix to use for log messages.
        /// If not specified or <c>null</c>, the name of the entry assembly will be used, as returned by <see cref="System.Reflection.Assembly.GetEntryAssembly"/>.
        /// If the entry assembly is <c>null</c>, the default prefix of "Bullseye" is used.
        /// </param>
        public static void RunTargetsWithoutExiting(IEnumerable<string> args, Func<Exception, bool> messageOnly = null, string logPrefix = null) =>
            RunTargetsWithoutExitingAsync(args, messageOnly, logPrefix).GetAwaiter().GetResult();

        /// <summary>
        /// Runs the previously specified targets and then calls <see cref="Environment.Exit(int)"/>.
        /// Any code which follows a call to this method will not be executed.
        /// </summary>
        /// <param name="args">The command line arguments.</param>
        /// <param name="messageOnly">
        /// A predicate that is called when an exception is thrown.
        /// Return <c>true</c> to display only the exception message instead instead of the full exception details.
        /// </param>
        /// <param name="logPrefix">
        /// The prefix to use for log messages.
        /// If not specified or <c>null</c>, the name of the entry assembly will be used, as returned by <see cref="System.Reflection.Assembly.GetEntryAssembly"/>.
        /// If the entry assembly is <c>null</c>, the default prefix of "Bullseye" is used.
        /// </param>
        public static void RunTargetsAndExit(IEnumerable<string> args, Func<Exception, bool> messageOnly = null, string logPrefix = null) =>
            RunTargetsAndExitAsync(args, messageOnly, logPrefix).GetAwaiter().GetResult();

        /// <summary>
        /// Adds a target which depends on other targets and performs an action.
        /// </summary>
        /// <param name="name">The name of the target.</param>
        /// <param name="dependsOn">The names of the targets on which the target depends.</param>
        /// <param name="action">The action performed by the target.</param>
        public void Add(string name, IEnumerable<string> dependsOn, Action action) =>
            this.Add(
                name,
                dependsOn,
                action == null
                    ? (Func<Task>)null
                    : () => Task.Run(action.Invoke));

        /// <summary>
        /// Adds a target which depends on other targets and performs an action for each item in a list of inputs.
        /// </summary>
        /// <typeparam name="TInput">The type of input required by <paramref name="action"/>.</typeparam>
        /// <param name="name">The name of the target.</param>
        /// <param name="dependsOn">The names of the targets on which the target depends.</param>
        /// <param name="forEach">The list of inputs to pass to <paramref name="action"/>.</param>
        /// <param name="action">The action performed by the target for each input in <paramref name="forEach"/>.</param>
        public void Add<TInput>(string name, IEnumerable<string> dependsOn, IEnumerable<TInput> forEach, Action<TInput> action) =>
            this.Add(
                name,
                dependsOn,
                forEach,
                action == null
                    ? (Func<TInput, Task>)null
                    : input => Task.Run(() => action.Invoke(input)));

        /// <summary>
        /// Runs the targets.
        /// In most cases, <see cref="RunAndExit(IEnumerable{string}, Func{Exception, bool}, string)"/> should be used instead of this method.
        /// This method should only be used if continued code execution after running targets is specifically required.
        /// </summary>
        /// <param name="args">The command line arguments.</param>
        /// <param name="messageOnly">
        /// A predicate that is called when an exception is thrown.
        /// Return <c>true</c> to display only the exception message instead instead of the full exception details.
        /// </param>
        /// <param name="logPrefix">
        /// The prefix to use for log messages.
        /// If not specified or <c>null</c>, the name of the entry assembly will be used, as returned by <see cref="System.Reflection.Assembly.GetEntryAssembly"/>.
        /// If the entry assembly is <c>null</c>, the default prefix of "Bullseye" is used.
        /// </param>
        public void RunWithoutExiting(IEnumerable<string> args, Func<Exception, bool> messageOnly = null, string logPrefix = null) =>
            this.targetCollection.RunAsync(args, messageOnly, logPrefix).GetAwaiter().GetResult();

        /// <summary>
        /// Runs the targets and then calls <see cref="Environment.Exit(int)"/>.
        /// Any code which follows a call to this method will not be executed.
        /// </summary>
        /// <param name="args">The command line arguments.</param>
        /// <param name="messageOnly">
        /// A predicate that is called when an exception is thrown.
        /// Return <c>true</c> to display only the exception message instead instead of the full exception details.
        /// </param>
        /// <param name="logPrefix">
        /// The prefix to use for log messages.
        /// If not specified or <c>null</c>, the name of the entry assembly will be used, as returned by <see cref="System.Reflection.Assembly.GetEntryAssembly"/>.
        /// If the entry assembly is <c>null</c>, the default prefix of "Bullseye" is used.
        /// </param>
        public void RunAndExit(IEnumerable<string> args, Func<Exception, bool> messageOnly = null, string logPrefix = null) =>
            this.targetCollection.RunAndExitAsync(args, messageOnly, logPrefix).GetAwaiter().GetResult();
    }
}
