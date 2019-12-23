namespace BullseyeSmokeTester.McMaster
{
    using System.Linq;
    using Bullseye;
    using global::McMaster.Extensions.CommandLineUtils;
    using static Bullseye.Targets;

    internal static class Program
    {
        private static int Main(string[] args)
        {
            var app = new CommandLineApplication() { UsePagerForHelpText = false };
            app.HelpOption();
            var foo = app.Option<string>("-f|--foo <foo>", "A value used for something.", CommandOptionType.SingleValue);

            // translate from Bullseye to McMaster.Extensions.CommandLineUtils
            var targets = app.Argument("targets", "The targets to run or list.", true);
            foreach (var option in Options.Definitions)
            {
                app.Option((option.ShortName != null ? $"{option.ShortName}|" : "") + option.LongName, option.Description, CommandOptionType.NoValue);
            }

            app.OnExecute(() =>
            {
                // translate from McMaster.Extensions.CommandLineUtils to Bullseye
                var targets = app.Arguments[0].Values;
                var options = new Options(Options.Definitions.Select(d => (d.LongName, app.Options.Single(o => "--" + o.LongName == d.LongName).HasValue())));

                Target("build", () => System.Console.WriteLine($"foo = {foo.Value()}"));

                Target("default", DependsOn("build"));

                RunTargetsAndExit(targets, options);
            });

            return app.Execute(args);
        }
    }
}
