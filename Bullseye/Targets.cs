namespace Bullseye
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Bullseye.Internal;

    /// <summary>
    /// Provides methods for defining and running targets.
    /// </summary>
    public static partial class Targets
    {
        private static readonly TargetCollection targets = new TargetCollection();

        /// <summary>
        /// Cosmetic method for defining an array of <see cref="string"/>.
        /// </summary>
        /// <param name="dependencies">The names of the targets on which the current target depends.</param>
        /// <returns>The specified <paramref name="dependencies"/>.</returns>
        public static string[] DependsOn(params string[] dependencies) => dependencies;

        /// <summary>
        /// Cosmetic method for defining an array of <typeparamref name="TInput"/>.
        /// </summary>
        /// <typeparam name="TInput">The type of input required by the action of the current target.</typeparam>
        /// <param name="inputs">The list of inputs, each to be passed to the action of the current target.</param>
        /// <returns>The specified <paramref name="inputs"/>.</returns>
        public static TInput[] ForEach<TInput>(params TInput[] inputs) => inputs;

        /// <summary>
        /// Defines a target which depends on other targets.
        /// </summary>
        /// <param name="name">The name of the target.</param>
        /// <param name="dependsOn">The names of the targets on which the target depends.</param>
        public static void Target(string name, IEnumerable<string> dependsOn) =>
            targets.Add(new Target(name, dependsOn));

        /// <summary>
        /// Defines a target which depends on other targets.
        /// </summary>
        /// <param name="name">The name of the target.</param>
        /// <param name="dependsOn">The names of the targets on which the target depends.</param>
        /// <param name="context">Shared build context.</param>
        public static void Target(string name, IEnumerable<string> dependsOn, IBuildContext context) =>
            targets.Add(new Target(name, dependsOn, context));

        /// <summary>
        /// Defines a target which depends on other targets and performs an action.
        /// </summary>
        /// <param name="name">The name of the target.</param>
        /// <param name="dependsOn">The names of the targets on which the target depends.</param>
        /// <param name="action">The action performed by the target.</param>
        public static void Target(string name, IEnumerable<string> dependsOn, Func<Task> action) =>
            targets.Add(new ActionTarget(name, dependsOn, action));

        /// <summary>
        /// Defines a target which depends on other targets and performs an action.
        /// </summary>
        /// <param name="name">The name of the target.</param>
        /// <param name="dependsOn">The names of the targets on which the target depends.</param>
        /// <param name="action">The action performed by the target.</param>
        public static void Target(string name, IEnumerable<string> dependsOn, Func<IBuildContext, Task> action) =>
            targets.Add(new ActionTarget(name, dependsOn, action, default));

        /// <summary>
        /// Defines a target which depends on other targets and performs an action.
        /// </summary>
        /// <param name="name">The name of the target.</param>
        /// <param name="dependsOn">The names of the targets on which the target depends.</param>
        /// <param name="action">The action performed by the target.</param>
        /// <param name="context">Shared build context.</param>
        public static void Target(string name, IEnumerable<string> dependsOn, Func<IBuildContext, Task> action, IBuildContext context) =>
            targets.Add(new ActionTarget(name, dependsOn, action, context));

        /// <summary>
        /// Defines a target which depends on other targets and performs an action for each item in a list of inputs.
        /// </summary>
        /// <typeparam name="TInput">The type of input required by <paramref name="action"/>.</typeparam>
        /// <param name="name">The name of the target.</param>
        /// <param name="dependsOn">The names of the targets on which the target depends.</param>
        /// <param name="forEach">The list of inputs to pass to <paramref name="action"/>.</param>
        /// <param name="action">The action performed by the target for each input in <paramref name="forEach"/>.</param>
        public static void Target<TInput>(string name, IEnumerable<string> dependsOn, IEnumerable<TInput> forEach, Func<TInput, Task> action) =>
            targets.Add(new ActionTarget<TInput>(name, dependsOn, forEach, action));

        /// <summary>
        /// Defines a target which depends on other targets and performs an action for each item in a list of inputs.
        /// </summary>
        /// <typeparam name="TInput">The type of input required by <paramref name="action"/>.</typeparam>
        /// <param name="name">The name of the target.</param>
        /// <param name="dependsOn">The names of the targets on which the target depends.</param>
        /// <param name="forEach">The list of inputs to pass to <paramref name="action"/>.</param>
        /// <param name="action">The action performed by the target for each input in <paramref name="forEach"/>.</param>
        /// <param name="context">Shared build context.</param>
        public static void Target<TInput>(string name, IEnumerable<string> dependsOn, IEnumerable<TInput> forEach, Func<TInput, IBuildContext, Task> action, IBuildContext context) =>
            targets.Add(new ActionTarget<TInput>(name, dependsOn, forEach, action, context));

        /// <summary>
        /// Runs the previously specified targets.
        /// In most cases, <see cref="RunTargetsAndExitAsync(IEnumerable{string}, Func{Exception, bool}, string, IBuildContext)"/> should be used instead of this method.
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
        /// <param name="context">Shared build context.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous running of the targets.</returns>
        public static Task RunTargetsWithoutExitingAsync(IEnumerable<string> args, Func<Exception, bool> messageOnly = null, string logPrefix = null, IBuildContext context = null) =>
            targets.RunAsync(args, messageOnly, logPrefix, context);

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
        /// <param name="context">Shared build context.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous running of the targets.</returns>
        public static Task RunTargetsAndExitAsync(IEnumerable<string> args, Func<Exception, bool> messageOnly = null, string logPrefix = null, IBuildContext context = null) =>
            targets.RunAndExitAsync(args, messageOnly, logPrefix, context);
    }
}
