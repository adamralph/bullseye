namespace Bullseye.Internal;

public static class ArgsParser
{
    private static readonly IReadOnlyList<string> HelpOptions = ["--help", "-help", "/help", "-h", "/h", "-?", "/?",];

    public static (IReadOnlyList<string> Targets, Options Options, IReadOnlyList<string> UnknownOptions, bool showHelp)
        Parse(IReadOnlyCollection<string> args)
    {
        var nonHelpArgs = args.Where(arg => !HelpOptions.Contains(arg, StringComparer.OrdinalIgnoreCase)).ToList();

        var readResult = OptionsReader.Read(nonHelpArgs.Where(IsNotTarget));
        var options = new Options
        {
            Clear = readResult.Clear,
            DryRun = readResult.DryRun,
            Host = readResult.Host,
            ListDependencies = readResult.ListDependencies,
            ListInputs = readResult.ListInputs,
            ListTargets = readResult.ListTargets,
            ListTree = readResult.ListTree,
            NoColor = readResult.NoColor,
            NoExtendedChars = readResult.NoExtendedChars,
            Parallel = readResult.Parallel,
            SkipDependencies = readResult.SkipDependencies,
            Verbose = readResult.Verbose,
        };

        return (
            nonHelpArgs.Where(IsTarget).ToList(),
            options,
            readResult.UnknownOptions,
            showHelp: nonHelpArgs.Count != args.Count);
    }

    private static bool IsTarget(string arg) => !arg.StartsWith('-');

    private static bool IsNotTarget(string arg) => !IsTarget(arg);
}
