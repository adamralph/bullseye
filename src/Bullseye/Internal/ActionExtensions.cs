namespace Bullseye.Internal;

internal static class ActionExtensions
{
    public static Func<Task> ToFunc(this Action action) => () => Task.Run(action.Invoke);

    public static Func<T, Task> ToFunc<T>(this Action<T> action) => obj => Task.Run(() => action.Invoke(obj));
}
