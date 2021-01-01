namespace Bullseye
{
    /// <summary>
    /// Provides methods for defining and running targets.
    /// </summary>
    public partial class Targets
    {
        private static readonly Targets instance = new Targets();

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
    }
}
