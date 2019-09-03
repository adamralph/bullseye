namespace Bullseye
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public static partial class Targets
    {
        /// <summary>
        /// Defines a target which performs an action.
        /// </summary>
        /// <param name="name">The name of the target.</param>
        /// <param name="action">The action performed by the target.</param>
        public static void Target(string name, Func<Task> action) => Target(name, default, action);

        /// <summary>
        /// Defines a target which performs an action.
        /// </summary>
        /// <param name="name">The name of the target.</param>
        /// <param name="action">The action performed by the target.</param>
        /// <param name="context">Shared build context.</param>
        public static void Target(string name, Func<IBuildContext, Task> action, IBuildContext context) => Target(name, default, action, context);

        /// <summary>
        /// Defines a target which performs an action.
        /// </summary>
        /// <param name="name">The name of the target.</param>
        /// <param name="action">The action performed by the target.</param>
        public static void Target(string name, Action action) => Target(name, default, action);

        /// <summary>
        /// Defines a target which performs an action.
        /// </summary>
        /// <param name="name">The name of the target.</param>
        /// <param name="action">The action performed by the target.</param>
        /// <param name="context">Shared build context.</param>
        public static void Target(string name, Action<IBuildContext> action, IBuildContext context = default) => Target(name, default, action, context);

        /// <summary>
        /// Defines a target which performs an action for each item in a list of inputs.
        /// </summary>
        /// <typeparam name="TInput">The type of input required by <paramref name="action"/>.</typeparam>
        /// <param name="name">The name of the target.</param>
        /// <param name="forEach">The list of inputs to pass to <paramref name="action"/>.</param>
        /// <param name="action">The action performed by the target for each input in <paramref name="forEach"/>.</param>
        public static void Target<TInput>(string name, IEnumerable<TInput> forEach, Func<TInput, Task> action) =>
            Target(name, default, forEach, action);

        /// <summary>
        /// Defines a target which performs an action for each item in a list of inputs.
        /// </summary>
        /// <typeparam name="TInput">The type of input required by <paramref name="action"/>.</typeparam>
        /// <param name="name">The name of the target.</param>
        /// <param name="forEach">The list of inputs to pass to <paramref name="action"/>.</param>
        /// <param name="action">The action performed by the target for each input in <paramref name="forEach"/>.</param>
        /// <param name="context">Shared build context.</param>
        public static void Target<TInput>(string name, IEnumerable<TInput> forEach, Func<TInput, IBuildContext, Task> action, IBuildContext context) =>
            Target(name, default, forEach, action, context);

        /// <summary>
        /// Defines a target which performs an action for each item in a list of inputs.
        /// </summary>
        /// <typeparam name="TInput">The type of input required by <paramref name="action"/>.</typeparam>
        /// <param name="name">The name of the target.</param>
        /// <param name="forEach">The list of inputs to pass to <paramref name="action"/>.</param>
        /// <param name="action">The action performed by the target for each input in <paramref name="forEach"/>.</param>
        public static void Target<TInput>(string name, IEnumerable<TInput> forEach, Action<TInput> action) =>
            Target(name, default, forEach, action);

        /// <summary>
        /// Defines a target which performs an action for each item in a list of inputs.
        /// </summary>
        /// <typeparam name="TInput">The type of input required by <paramref name="action"/>.</typeparam>
        /// <param name="name">The name of the target.</param>
        /// <param name="forEach">The list of inputs to pass to <paramref name="action"/>.</param>
        /// <param name="action">The action performed by the target for each input in <paramref name="forEach"/>.</param>
        /// <param name="context">Shared build context.</param>
        public static void Target<TInput>(string name, IEnumerable<TInput> forEach, Action<TInput, IBuildContext> action, IBuildContext context) =>
            Target(name, default, forEach, action, context);
    }
}
