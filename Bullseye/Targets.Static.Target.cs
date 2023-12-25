using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Bullseye;

/// <summary>
/// Provides methods for defining and running targets.
/// </summary>
public partial class Targets
{
    /// <summary>
    /// Defines a target which performs an action.
    /// </summary>
    /// <param name="name">The name of the target.</param>
    /// <param name="description">The description of the target.</param>
    /// <param name="action">The action performed by the target.</param>
    public static void Target(string name, string description, Action action) =>
        instance.Add(name, description, action);

    /// <summary>
    /// Defines a target which depends on other targets.
    /// </summary>
    /// <param name="name">The name of the target.</param>
    /// <param name="description">The description of the target.</param>
    /// <param name="dependsOn">The names of the targets on which the target depends.</param>
    public static void Target(string name, string description, IEnumerable<string> dependsOn) =>
        instance.Add(name, description, dependsOn);

    /// <summary>
    /// Defines a target which depends on other targets and performs an action.
    /// </summary>
    /// <param name="name">The name of the target.</param>
    /// <param name="description">The description of the target.</param>
    /// <param name="dependsOn">The names of the targets on which the target depends.</param>
    /// <param name="action">The action performed by the target.</param>
    public static void Target(string name, string description, IEnumerable<string> dependsOn, Action action) =>
        instance.Add(name, description, dependsOn, action);

    /// <summary>
    /// Defines a target which depends on other targets and performs an action.
    /// </summary>
    /// <param name="name">The name of the target.</param>
    /// <param name="description">The description of the target.</param>
    /// <param name="dependsOn">The names of the targets on which the target depends.</param>
    /// <param name="action">The action performed by the target.</param>
    public static void Target(string name, string description, IEnumerable<string> dependsOn, Func<Task> action) =>
        instance.Add(name, description, dependsOn, action);

    /// <summary>
    /// Defines a target which performs an action.
    /// </summary>
    /// <param name="name">The name of the target.</param>
    /// <param name="description">The description of the target.</param>
    /// <param name="action">The action performed by the target.</param>
    public static void Target(string name, string description, Func<Task> action) =>
        instance.Add(name, description, action);

    /// <summary>
    /// Defines a target which performs an action.
    /// </summary>
    /// <param name="name">The name of the target.</param>
    /// <param name="action">The action performed by the target.</param>
    public static void Target(string name, Action action) =>
        instance.Add(name, action);

    /// <summary>
    /// Defines a target which depends on other targets.
    /// </summary>
    /// <param name="name">The name of the target.</param>
    /// <param name="dependsOn">The names of the targets on which the target depends.</param>
    public static void Target(string name, IEnumerable<string> dependsOn) =>
        instance.Add(name, dependsOn);

    /// <summary>
    /// Defines a target which depends on other targets and performs an action.
    /// </summary>
    /// <param name="name">The name of the target.</param>
    /// <param name="dependsOn">The names of the targets on which the target depends.</param>
    /// <param name="action">The action performed by the target.</param>
    public static void Target(string name, IEnumerable<string> dependsOn, Action action) =>
        instance.Add(name, dependsOn, action);

    /// <summary>
    /// Defines a target which depends on other targets and performs an action.
    /// </summary>
    /// <param name="name">The name of the target.</param>
    /// <param name="dependsOn">The names of the targets on which the target depends.</param>
    /// <param name="action">The action performed by the target.</param>
    public static void Target(string name, IEnumerable<string> dependsOn, Func<Task> action) =>
        instance.Add(name, dependsOn, action);

    /// <summary>
    /// Defines a target which performs an action.
    /// </summary>
    /// <param name="name">The name of the target.</param>
    /// <param name="action">The action performed by the target.</param>
    public static void Target(string name, Func<Task> action) =>
        instance.Add(name, action);

    /// <summary>
    /// Defines a target which depends on other targets and performs an action for each item in a list of inputs.
    /// </summary>
    /// <typeparam name="TInput">The type of input required by <paramref name="action"/>.</typeparam>
    /// <param name="name">The name of the target.</param>
    /// <param name="description">The description of the target.</param>
    /// <param name="dependsOn">The names of the targets on which the target depends.</param>
    /// <param name="forEach">The list of inputs to pass to <paramref name="action"/>.</param>
    /// <param name="action">The action performed by the target for each input in <paramref name="forEach"/>.</param>
    public static void Target<TInput>(string name, string description, IEnumerable<string> dependsOn, IEnumerable<TInput> forEach, Action<TInput> action) =>
        instance.Add(name, description, dependsOn, forEach, action);

    /// <summary>
    /// Defines a target which depends on other targets and performs an action for each item in a list of inputs.
    /// </summary>
    /// <typeparam name="TInput">The type of input required by <paramref name="action"/>.</typeparam>
    /// <param name="name">The name of the target.</param>
    /// <param name="description">The description of the target.</param>
    /// <param name="dependsOn">The names of the targets on which the target depends.</param>
    /// <param name="forEach">The list of inputs to pass to <paramref name="action"/>.</param>
    /// <param name="action">The action performed by the target for each input in <paramref name="forEach"/>.</param>
    public static void Target<TInput>(string name, string description, IEnumerable<string> dependsOn, IEnumerable<TInput> forEach, Func<TInput, Task> action) =>
        instance.Add(name, description, dependsOn, forEach, action);

    /// <summary>
    /// Defines a target which performs an action for each item in a list of inputs.
    /// </summary>
    /// <typeparam name="TInput">The type of input required by <paramref name="action"/>.</typeparam>
    /// <param name="name">The name of the target.</param>
    /// <param name="description">The description of the target.</param>
    /// <param name="forEach">The list of inputs to pass to <paramref name="action"/>.</param>
    /// <param name="action">The action performed by the target for each input in <paramref name="forEach"/>.</param>
    public static void Target<TInput>(string name, string description, IEnumerable<TInput> forEach, Action<TInput> action) =>
        instance.Add(name, description, forEach, action);

    /// <summary>
    /// Defines a target which performs an action for each item in a list of inputs.
    /// </summary>
    /// <typeparam name="TInput">The type of input required by <paramref name="action"/>.</typeparam>
    /// <param name="name">The name of the target.</param>
    /// <param name="description">The description of the target.</param>
    /// <param name="forEach">The list of inputs to pass to <paramref name="action"/>.</param>
    /// <param name="action">The action performed by the target for each input in <paramref name="forEach"/>.</param>
    public static void Target<TInput>(string name, string description, IEnumerable<TInput> forEach, Func<TInput, Task> action) =>
        instance.Add(name, description, forEach, action);

    /// <summary>
    /// Defines a target which depends on other targets and performs an action for each item in a list of inputs.
    /// </summary>
    /// <typeparam name="TInput">The type of input required by <paramref name="action"/>.</typeparam>
    /// <param name="name">The name of the target.</param>
    /// <param name="dependsOn">The names of the targets on which the target depends.</param>
    /// <param name="forEach">The list of inputs to pass to <paramref name="action"/>.</param>
    /// <param name="action">The action performed by the target for each input in <paramref name="forEach"/>.</param>
    public static void Target<TInput>(string name, IEnumerable<string> dependsOn, IEnumerable<TInput> forEach, Action<TInput> action) =>
        instance.Add(name, dependsOn, forEach, action);

    /// <summary>
    /// Defines a target which depends on other targets and performs an action for each item in a list of inputs.
    /// </summary>
    /// <typeparam name="TInput">The type of input required by <paramref name="action"/>.</typeparam>
    /// <param name="name">The name of the target.</param>
    /// <param name="dependsOn">The names of the targets on which the target depends.</param>
    /// <param name="forEach">The list of inputs to pass to <paramref name="action"/>.</param>
    /// <param name="action">The action performed by the target for each input in <paramref name="forEach"/>.</param>
    public static void Target<TInput>(string name, IEnumerable<string> dependsOn, IEnumerable<TInput> forEach, Func<TInput, Task> action) =>
        instance.Add(name, dependsOn, forEach, action);

    /// <summary>
    /// Defines a target which performs an action for each item in a list of inputs.
    /// </summary>
    /// <typeparam name="TInput">The type of input required by <paramref name="action"/>.</typeparam>
    /// <param name="name">The name of the target.</param>
    /// <param name="forEach">The list of inputs to pass to <paramref name="action"/>.</param>
    /// <param name="action">The action performed by the target for each input in <paramref name="forEach"/>.</param>
    public static void Target<TInput>(string name, IEnumerable<TInput> forEach, Action<TInput> action) =>
        instance.Add(name, forEach, action);

    /// <summary>
    /// Defines a target which performs an action for each item in a list of inputs.
    /// </summary>
    /// <typeparam name="TInput">The type of input required by <paramref name="action"/>.</typeparam>
    /// <param name="name">The name of the target.</param>
    /// <param name="forEach">The list of inputs to pass to <paramref name="action"/>.</param>
    /// <param name="action">The action performed by the target for each input in <paramref name="forEach"/>.</param>
    public static void Target<TInput>(string name, IEnumerable<TInput> forEach, Func<TInput, Task> action) =>
        instance.Add(name, forEach, action);
}
