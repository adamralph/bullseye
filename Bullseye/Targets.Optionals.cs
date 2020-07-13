namespace Bullseye
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public partial class Targets
    {
        /// <summary>
        /// Defines a target which performs an action.
        /// </summary>
        /// <param name="name">The name of the target.</param>
        /// <param name="action">The action performed by the target.</param>
        /// <param name="teardown">The action to teardown any resources set up by the target.</param>
        public static void Target(string name, Func<Task> action, Func<Task> teardown = null) => Target(name, null, action, teardown);

        /// <summary>
        /// Defines a target which performs an action.
        /// </summary>
        /// <param name="name">The name of the target.</param>
        /// <param name="action">The action performed by the target.</param>
        /// <param name="teardown">The action to teardown any resources set up by the target.</param>
        public static void Target(string name, Action action, Action teardown = null) => Target(name, null, action, teardown);

        /// <summary>
        /// Defines a target which performs an action for each item in a list of inputs.
        /// </summary>
        /// <typeparam name="TInput">The type of input required by <paramref name="action"/>.</typeparam>
        /// <param name="name">The name of the target.</param>
        /// <param name="forEach">The list of inputs to pass to <paramref name="action"/>.</param>
        /// <param name="action">The action performed by the target for each input in <paramref name="forEach"/>.</param>
        /// <param name="teardown">The action to teardown any resources set up by the target for each input in <paramref name="forEach"/>.</param>
        public static void Target<TInput>(string name, IEnumerable<TInput> forEach, Func<TInput, Task> action, Func<TInput, Task> teardown = null) => Target(name, null, forEach, action, teardown);

        /// <summary>
        /// Defines a target which performs an action for each item in a list of inputs.
        /// </summary>
        /// <typeparam name="TInput">The type of input required by <paramref name="action"/>.</typeparam>
        /// <param name="name">The name of the target.</param>
        /// <param name="forEach">The list of inputs to pass to <paramref name="action"/>.</param>
        /// <param name="action">The action performed by the target for each input in <paramref name="forEach"/>.</param>
        /// <param name="teardown">The action to teardown any resources set up by the target for each input in <paramref name="forEach"/>.</param>
        public static void Target<TInput>(string name, IEnumerable<TInput> forEach, Action<TInput> action, Action<TInput> teardown = null) => Target(name, null, forEach, action, teardown);

        /// <summary>
        /// Adds a target which performs an action.
        /// </summary>
        /// <param name="name">The name of the target.</param>
        /// <param name="action">The action performed by the target.</param>
        /// <param name="teardown">The action to teardown any resources set up by the target.</param>
        public void Add(string name, Func<Task> action, Func<Task> teardown = null) => this.Add(name, null, action, teardown);

        /// <summary>
        /// Adds a target which performs an action.
        /// </summary>
        /// <param name="name">The name of the target.</param>
        /// <param name="action">The action performed by the target.</param>
        /// <param name="teardown">The action to teardown any resources set up by the target.</param>
        public void Add(string name, Action action, Action teardown = null) => this.Add(name, null, action, teardown);

        /// <summary>
        /// Adds a target which performs an action for each item in a list of inputs.
        /// </summary>
        /// <typeparam name="TInput">The type of input required by <paramref name="action"/>.</typeparam>
        /// <param name="name">The name of the target.</param>
        /// <param name="forEach">The list of inputs to pass to <paramref name="action"/>.</param>
        /// <param name="action">The action performed by the target for each input in <paramref name="forEach"/>.</param>
        /// <param name="teardown">The action to teardown any resources set up by the target for each input in <paramref name="forEach"/>.</param>
        public void Add<TInput>(string name, IEnumerable<TInput> forEach, Func<TInput, Task> action, Func<TInput, Task> teardown = null) => this.Add(name, null, forEach, action, teardown);

        /// <summary>
        /// Adds a target which performs an action for each item in a list of inputs.
        /// </summary>
        /// <typeparam name="TInput">The type of input required by <paramref name="action"/>.</typeparam>
        /// <param name="name">The name of the target.</param>
        /// <param name="forEach">The list of inputs to pass to <paramref name="action"/>.</param>
        /// <param name="action">The action performed by the target for each input in <paramref name="forEach"/>.</param>
        /// <param name="teardown">The action to teardown any resources set up by the target for each input in <paramref name="forEach"/>.</param>
        public void Add<TInput>(string name, IEnumerable<TInput> forEach, Action<TInput> action, Action<TInput> teardown = null) => this.Add(name, null, forEach, action, teardown);
    }
}
