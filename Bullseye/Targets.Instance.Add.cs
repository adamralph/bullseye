using Bullseye.Internal;

namespace Bullseye;

/// <summary>
/// Provides methods for defining and running targets.
/// </summary>
public partial class Targets
{
    /// <summary>
    /// Adds a target which performs an action.
    /// </summary>
    /// <param name="name">The name of the target.</param>
    /// <param name="description">The description of the target.</param>
    /// <param name="action">The action performed by the target.</param>
    public void Add(string name, string description, Action action) =>
        this.targetCollection.Add(new ActionTarget(name, description, [], action.ToAsync()));

    /// <summary>
    /// Adds a target which depends on other targets.
    /// </summary>
    /// <param name="name">The name of the target.</param>
    /// <param name="description">The description of the target.</param>
    /// <param name="dependsOn">The names of the targets on which the target depends.</param>
    public void Add(string name, string description, IEnumerable<string> dependsOn) =>
        this.targetCollection.Add(new Target(name, description, dependsOn));

    /// <summary>
    /// Adds a target which depends on other targets and performs an action.
    /// </summary>
    /// <param name="name">The name of the target.</param>
    /// <param name="description">The description of the target.</param>
    /// <param name="dependsOn">The names of the targets on which the target depends.</param>
    /// <param name="action">The action performed by the target.</param>
    public void Add(string name, string description, IEnumerable<string> dependsOn, Action action) =>
        this.targetCollection.Add(new ActionTarget(name, description, dependsOn, action.ToAsync()));

    /// <summary>
    /// Adds a target which depends on other targets and performs an action.
    /// </summary>
    /// <param name="name">The name of the target.</param>
    /// <param name="description">The description of the target.</param>
    /// <param name="dependsOn">The names of the targets on which the target depends.</param>
    /// <param name="action">The action performed by the target.</param>
    public void Add(string name, string description, IEnumerable<string> dependsOn, Func<Task> action) =>
        this.targetCollection.Add(new ActionTarget(name, description, dependsOn, action));

    /// <summary>
    /// Adds a target which performs an action.
    /// </summary>
    /// <param name="name">The name of the target.</param>
    /// <param name="description">The description of the target.</param>
    /// <param name="action">The action performed by the target.</param>
    public void Add(string name, string description, Func<Task> action) =>
        this.targetCollection.Add(new ActionTarget(name, description, [], action));

    /// <summary>
    /// Adds a target which performs an action.
    /// </summary>
    /// <param name="name">The name of the target.</param>
    /// <param name="action">The action performed by the target.</param>
    public void Add(string name, Action action) =>
        this.targetCollection.Add(new ActionTarget(name, "", [], action.ToAsync()));

    /// <summary>
    /// Adds a target which depends on other targets.
    /// </summary>
    /// <param name="name">The name of the target.</param>
    /// <param name="dependsOn">The names of the targets on which the target depends.</param>
    public void Add(string name, IEnumerable<string> dependsOn) =>
        this.targetCollection.Add(new Target(name, "", dependsOn));

    /// <summary>
    /// Adds a target which depends on other targets and performs an action.
    /// </summary>
    /// <param name="name">The name of the target.</param>
    /// <param name="dependsOn">The names of the targets on which the target depends.</param>
    /// <param name="action">The action performed by the target.</param>
    public void Add(string name, IEnumerable<string> dependsOn, Action action) =>
        this.targetCollection.Add(new ActionTarget(name, "", dependsOn, action.ToAsync()));

    /// <summary>
    /// Adds a target which depends on other targets and performs an action.
    /// </summary>
    /// <param name="name">The name of the target.</param>
    /// <param name="dependsOn">The names of the targets on which the target depends.</param>
    /// <param name="action">The action performed by the target.</param>
    public void Add(string name, IEnumerable<string> dependsOn, Func<Task> action) =>
        this.targetCollection.Add(new ActionTarget(name, "", dependsOn, action));

    /// <summary>
    /// Adds a target which performs an action.
    /// </summary>
    /// <param name="name">The name of the target.</param>
    /// <param name="action">The action performed by the target.</param>
    public void Add(string name, Func<Task> action) =>
        this.targetCollection.Add(new ActionTarget(name, "", [], action));

    /// <summary>
    /// Adds a target which depends on other targets and performs an action for each item in a list of inputs.
    /// </summary>
    /// <typeparam name="TInput">The type of input required by <paramref name="action"/>.</typeparam>
    /// <param name="name">The name of the target.</param>
    /// <param name="description">The description of the target.</param>
    /// <param name="dependsOn">The names of the targets on which the target depends.</param>
    /// <param name="forEach">The list of inputs to pass to <paramref name="action"/>.</param>
    /// <param name="action">The action performed by the target for each input in <paramref name="forEach"/>.</param>
    public void Add<TInput>(string name, string description, IEnumerable<string> dependsOn, IEnumerable<TInput> forEach, Action<TInput> action) =>
        this.targetCollection.Add(new ActionTarget<TInput>(name, description, dependsOn, forEach, action.ToAsync()));

    /// <summary>
    /// Adds a target which depends on other targets and performs an action for each item in a list of inputs.
    /// </summary>
    /// <typeparam name="TInput">The type of input required by <paramref name="action"/>.</typeparam>
    /// <param name="name">The name of the target.</param>
    /// <param name="description">The description of the target.</param>
    /// <param name="dependsOn">The names of the targets on which the target depends.</param>
    /// <param name="forEach">The list of inputs to pass to <paramref name="action"/>.</param>
    /// <param name="action">The action performed by the target for each input in <paramref name="forEach"/>.</param>
    public void Add<TInput>(string name, string description, IEnumerable<string> dependsOn, IEnumerable<TInput> forEach, Func<TInput, Task> action) =>
        this.targetCollection.Add(new ActionTarget<TInput>(name, description, dependsOn, forEach, action));

    /// <summary>
    /// Adds a target which performs an action for each item in a list of inputs.
    /// </summary>
    /// <typeparam name="TInput">The type of input required by <paramref name="action"/>.</typeparam>
    /// <param name="name">The name of the target.</param>
    /// <param name="description">The description of the target.</param>
    /// <param name="forEach">The list of inputs to pass to <paramref name="action"/>.</param>
    /// <param name="action">The action performed by the target for each input in <paramref name="forEach"/>.</param>
    public void Add<TInput>(string name, string description, IEnumerable<TInput> forEach, Action<TInput> action) =>
        this.targetCollection.Add(new ActionTarget<TInput>(name, description, [], forEach, action.ToAsync()));

    /// <summary>
    /// Adds a target which performs an action for each item in a list of inputs.
    /// </summary>
    /// <typeparam name="TInput">The type of input required by <paramref name="action"/>.</typeparam>
    /// <param name="name">The name of the target.</param>
    /// <param name="description">The description of the target.</param>
    /// <param name="forEach">The list of inputs to pass to <paramref name="action"/>.</param>
    /// <param name="action">The action performed by the target for each input in <paramref name="forEach"/>.</param>
    public void Add<TInput>(string name, string description, IEnumerable<TInput> forEach, Func<TInput, Task> action) =>
        this.targetCollection.Add(new ActionTarget<TInput>(name, description, [], forEach, action));

    /// <summary>
    /// Adds a target which depends on other targets and performs an action for each item in a list of inputs.
    /// </summary>
    /// <typeparam name="TInput">The type of input required by <paramref name="action"/>.</typeparam>
    /// <param name="name">The name of the target.</param>
    /// <param name="dependsOn">The names of the targets on which the target depends.</param>
    /// <param name="forEach">The list of inputs to pass to <paramref name="action"/>.</param>
    /// <param name="action">The action performed by the target for each input in <paramref name="forEach"/>.</param>
    public void Add<TInput>(string name, IEnumerable<string> dependsOn, IEnumerable<TInput> forEach, Action<TInput> action) =>
        this.targetCollection.Add(new ActionTarget<TInput>(name, "", dependsOn, forEach, action.ToAsync()));

    /// <summary>
    /// Adds a target which depends on other targets and performs an action for each item in a list of inputs.
    /// </summary>
    /// <typeparam name="TInput">The type of input required by <paramref name="action"/>.</typeparam>
    /// <param name="name">The name of the target.</param>
    /// <param name="dependsOn">The names of the targets on which the target depends.</param>
    /// <param name="forEach">The list of inputs to pass to <paramref name="action"/>.</param>
    /// <param name="action">The action performed by the target for each input in <paramref name="forEach"/>.</param>
    public void Add<TInput>(string name, IEnumerable<string> dependsOn, IEnumerable<TInput> forEach, Func<TInput, Task> action) =>
        this.targetCollection.Add(new ActionTarget<TInput>(name, "", dependsOn, forEach, action));

    /// <summary>
    /// Adds a target which performs an action for each item in a list of inputs.
    /// </summary>
    /// <typeparam name="TInput">The type of input required by <paramref name="action"/>.</typeparam>
    /// <param name="name">The name of the target.</param>
    /// <param name="forEach">The list of inputs to pass to <paramref name="action"/>.</param>
    /// <param name="action">The action performed by the target for each input in <paramref name="forEach"/>.</param>
    public void Add<TInput>(string name, IEnumerable<TInput> forEach, Action<TInput> action) =>
        this.targetCollection.Add(new ActionTarget<TInput>(name, "", [], forEach, action.ToAsync()));

    /// <summary>
    /// Adds a target which performs an action for each item in a list of inputs.
    /// </summary>
    /// <typeparam name="TInput">The type of input required by <paramref name="action"/>.</typeparam>
    /// <param name="name">The name of the target.</param>
    /// <param name="forEach">The list of inputs to pass to <paramref name="action"/>.</param>
    /// <param name="action">The action performed by the target for each input in <paramref name="forEach"/>.</param>
    public void Add<TInput>(string name, IEnumerable<TInput> forEach, Func<TInput, Task> action) =>
        this.targetCollection.Add(new ActionTarget<TInput>(name, "", [], forEach, action));
}
