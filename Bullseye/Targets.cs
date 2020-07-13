namespace Bullseye
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Bullseye.Internal;

    /// <summary>
    /// Provides methods for defining and running targets.
    /// </summary>
    public partial class Targets
    {
        private readonly TargetCollection targetCollection = new TargetCollection();

        /// <summary>
        /// Adds a target which depends on other targets.
        /// </summary>
        /// <param name="name">The name of the target.</param>
        /// <param name="dependsOn">The names of the targets on which the target depends.</param>
        public void Add(string name, IEnumerable<string> dependsOn) =>
            this.targetCollection.Add(new Target(name, dependsOn));

        /// <summary>
        /// Adds a target which depends on other targets and performs an action.
        /// </summary>
        /// <param name="name">The name of the target.</param>
        /// <param name="dependsOn">The names of the targets on which the target depends.</param>
        /// <param name="action">The action performed by the target.</param>
        /// <param name="teardown">The action to teardown any resources set up by the target.</param>
        public void Add(string name, IEnumerable<string> dependsOn, Func<Task> action, Func<Task> teardown = null) =>
            this.targetCollection.Add(new ActionTarget(name, dependsOn, action, teardown));

        /// <summary>
        /// Adds a target which depends on other targets and performs an action for each item in a list of inputs.
        /// </summary>
        /// <typeparam name="TInput">The type of input required by <paramref name="action"/>.</typeparam>
        /// <param name="name">The name of the target.</param>
        /// <param name="dependsOn">The names of the targets on which the target depends.</param>
        /// <param name="forEach">The list of inputs to pass to <paramref name="action"/>.</param>
        /// <param name="action">The action performed by the target for each input in <paramref name="forEach"/>.</param>
        /// <param name="teardown">The action to teardown any resources set up by the target for each input in <paramref name="forEach"/>.</param>
        public void Add<TInput>(string name, IEnumerable<string> dependsOn, IEnumerable<TInput> forEach, Func<TInput, Task> action, Func<TInput, Task> teardown = null) =>
            this.targetCollection.Add(new ActionTarget<TInput>(name, dependsOn, forEach, action, teardown));

        /// <summary>
        /// Runs the targets.
        /// In most cases, <see cref="RunAndExitAsync(IEnumerable{string}, Func{Exception, bool}, string)"/> should be used instead of this method.
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
        /// <returns>A <see cref="Task"/> that represents the asynchronous running of the targets.</returns>
        public Task RunWithoutExitingAsync(IEnumerable<string> args, Func<Exception, bool> messageOnly = null, string logPrefix = null) =>
            this.targetCollection.RunAsync(args, messageOnly, logPrefix, false);

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
        /// <returns>A <see cref="Task"/> that represents the asynchronous running of the targets.</returns>
        public Task RunAndExitAsync(IEnumerable<string> args, Func<Exception, bool> messageOnly = null, string logPrefix = null) =>
            this.targetCollection.RunAsync(args, messageOnly, logPrefix, true);

        /// <summary>
        /// Runs the targets.
        /// In most cases, RunAndExitAsync should be used instead of this method.
        /// This method should only be used if continued code execution after running targets is specifically required.
        /// </summary>
        /// <param name="targets">The targets to run or list.</param>
        /// <param name="options">The options to use when running or listing targets.</param>
        /// <param name="messageOnly">
        /// A predicate that is called when an exception is thrown.
        /// Return <c>true</c> to display only the exception message instead instead of the full exception details.
        /// </param>
        /// <param name="logPrefix">
        /// The prefix to use for log messages.
        /// If not specified or <c>null</c>, the name of the entry assembly will be used, as returned by <see cref="System.Reflection.Assembly.GetEntryAssembly"/>.
        /// If the entry assembly is <c>null</c>, the default prefix of "Bullseye" is used.
        /// </param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous running of the targets.</returns>
        public Task RunWithoutExitingAsync(IEnumerable<string> targets, Options options, Func<Exception, bool> messageOnly = null, string logPrefix = null) =>
            this.targetCollection.RunAsync(targets, options, messageOnly, logPrefix, false);

        /// <summary>
        /// Runs the targets and then calls <see cref="Environment.Exit(int)"/>.
        /// Any code which follows a call to this method will not be executed.
        /// </summary>
        /// <param name="targets">The targets to run or list.</param>
        /// <param name="options">The options to use when running or listing targets.</param>
        /// <param name="messageOnly">
        /// A predicate that is called when an exception is thrown.
        /// Return <c>true</c> to display only the exception message instead instead of the full exception details.
        /// </param>
        /// <param name="logPrefix">
        /// The prefix to use for log messages.
        /// If not specified or <c>null</c>, the name of the entry assembly will be used, as returned by <see cref="System.Reflection.Assembly.GetEntryAssembly"/>.
        /// If the entry assembly is <c>null</c>, the default prefix of "Bullseye" is used.
        /// </param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous running of the targets.</returns>
        public Task RunAndExitAsync(IEnumerable<string> targets, Options options, Func<Exception, bool> messageOnly = null, string logPrefix = null) =>
            this.targetCollection.RunAsync(targets, options, messageOnly, logPrefix, true);
    }
}
