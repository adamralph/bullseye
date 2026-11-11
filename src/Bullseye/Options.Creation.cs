using Bullseye.Internal;

namespace Bullseye;

/// <summary>
/// The options to use when running or listing targets.
/// </summary>
#pragma warning disable CA1724 // Type names should not match namespaces
public partial class Options : IOptions
#pragma warning restore CA1724
{
    /// <summary>
    /// Constructs a new instance of <see cref="Options"/>.
    /// </summary>
    public Options()
    {
    }

    /// <summary>
    /// Constructs a new instance of <see cref="Options"/>.
    /// </summary>
    /// <param name="values">A list of named options and their values.</param>
    public Options(IEnumerable<(string Name, bool Value)> values)
    {
        var result = OptionsReader.Read(values.Where(option => option.Value).Select(option => option.Name));

        if (result.UnknownOptions.Count > 0)
        {
            throw new InvalidUsageException($"Unknown option{(result.UnknownOptions.Count > 1 ? "s" : "")} {result.UnknownOptions.Spaced()}.");
        }

        Clear = result.Clear;
        DryRun = result.DryRun;
        Host = result.Host;
        ListDependencies = result.ListDependencies;
        ListInputs = result.ListInputs;
        ListTargets = result.ListTargets;
        ListTree = result.ListTree;
        NoColor = result.NoColor;
        NoExtendedChars = result.NoExtendedChars;
        Parallel = result.Parallel;
        SkipDependencies = result.SkipDependencies;
        Verbose = result.Verbose;
    }
}
