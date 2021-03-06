using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable IDE0009 // Member access should be qualified.
#pragma warning disable RS0016 // Add public types and members to the declared API
namespace Bullseye.Internal
{
    public class Output
    {
        private readonly TextWriter writer;
        private readonly Palette p;
        private readonly string scriptExtension;

        public Output(TextWriter writer, Palette palette, OperatingSystem operatingSystem)
        {
            this.writer = writer;
            this.p = palette;
            this.scriptExtension = operatingSystem == OperatingSystem.Windows ? "cmd" : "sh";
        }

        public Task Usage(TargetCollection targets) => this.writer.WriteAsync(this.GetUsage(targets));

        public Task Targets(TargetCollection targets, List<string> rootTargets, int maxDepth, int maxDepthToShowInputs, bool listInputs) =>
            this.writer.WriteAsync(this.List(targets, rootTargets, maxDepth, maxDepthToShowInputs, listInputs, null));

        private string List(TargetCollection targets, List<string> rootTargets, int maxDepth, int maxDepthToShowInputs, bool listInputs, string startingPrefix)
        {
            var lines = new List<(string, string)>();

            foreach (var rootTarget in rootTargets)
            {
                Append(new List<string> { rootTarget }, new Stack<string>(), true, "", 0);
            }

            var maxColumn1Width = lines.Max(line => Palette.StripColours(line.Item1).Length);

            return string.Join("", lines.Select(line => $"{line.Item1.PadRight(maxColumn1Width + line.Item1.Length - Palette.StripColours(line.Item1).Length)}    {line.Item2}{Environment.NewLine}"));

            void Append(List<string> names, Stack<string> seenTargets, bool isRoot, string previousPrefix, int depth)
            {
                if (depth > maxDepth)
                {
                    return;
                }

                foreach (var item in names.Select((name, index) => new { name, index }))
                {
                    var circularDependency = seenTargets.Contains(item.name);

                    seenTargets.Push(item.name);

                    try
                    {
                        var prefix = isRoot
                            ? startingPrefix ?? ""
                            : $"{previousPrefix.Replace(p.TreeCorner, "  ").Replace(p.TreeFork, p.TreeDown)}{(item.index == names.Count - 1 ? p.TreeCorner : p.TreeFork)}";

                        var isMissing = !targets.Contains(item.name);

                        var line = $"{prefix}{p.Target}{item.name}";

                        if (isMissing)
                        {
                            lines.Add((line + $"{p.Reset} {p.Failed}(missing){p.Reset}", null));
                            continue;
                        }

                        if (circularDependency)
                        {
                            lines.Add((line + $"{p.Reset} {p.Failed}(circular dependency){p.Reset}", targets[item.name].Description));
                            continue;
                        }

                        lines.Add((line + p.Reset, targets[item.name].Description));

                        var target = targets[item.name];

                        if (listInputs && depth <= maxDepthToShowInputs && target is IHaveInputs hasInputs)
                        {
                            foreach (var inputItem in hasInputs.Inputs.Select((input, index) => new { input, index }))
                            {
                                var inputPrefix = $"{prefix.Replace(p.TreeCorner, "  ").Replace(p.TreeFork, p.TreeDown)}{(target.Dependencies.Any() && depth + 1 <= maxDepth ? p.TreeDown : "  ")}";

                                lines.Add(($"{inputPrefix}{p.Input}{inputItem.input}{p.Reset}", null));
                            }
                        }

                        Append(target.Dependencies, seenTargets, false, prefix, depth + 1);
                    }
                    finally
                    {
                        seenTargets.Pop();
                    }
                }
            }
        }

        // editorconfig-checker-disable
        private string GetUsage(TargetCollection targets) =>
$@"{p.Default}Usage:{p.Reset}
  {p.Invocation}[invocation]{p.Reset} {p.Option}[options]{p.Reset} {p.Target}[<targets>...]{p.Reset}

{p.Default}Arguments:{p.Reset}
  {p.Target}<targets>{p.Reset}    {p.Default}A list of targets to run or list. If not specified, the {p.Target}""default""{p.Default} target will be run, or all targets will be listed.{p.Reset}

{p.Default}Options:{p.Reset}
  {p.Option}-c{p.Default},{p.Reset} {p.Option}--clear{p.Reset}                {p.Default}Clear the console before execution{p.Reset}
  {p.Option}-n{p.Default},{p.Reset} {p.Option}--dry-run{p.Reset}              {p.Default}Do a dry run without executing actions{p.Reset}
  {p.Option}-d{p.Default},{p.Reset} {p.Option}--list-dependencies{p.Reset}    {p.Default}List all (or specified) targets and dependencies, then exit{p.Reset}
  {p.Option}-i{p.Default},{p.Reset} {p.Option}--list-inputs{p.Reset}          {p.Default}List all (or specified) targets and inputs, then exit{p.Reset}
  {p.Option}-l{p.Default},{p.Reset} {p.Option}--list-targets{p.Reset}         {p.Default}List all (or specified) targets, then exit{p.Reset}
  {p.Option}-t{p.Default},{p.Reset} {p.Option}--list-tree{p.Reset}            {p.Default}List all (or specified) targets and dependency trees, then exit{p.Reset}
  {p.Option}-N{p.Default},{p.Reset} {p.Option}--no-color{p.Reset}             {p.Default}Disable colored output{p.Reset}
  {p.Option}-E{p.Default},{p.Reset} {p.Option}--no-extended-chars{p.Reset}    {p.Default}Disable extended characters{p.Reset}
  {p.Option}-p{p.Default},{p.Reset} {p.Option}--parallel{p.Reset}             {p.Default}Run targets in parallel{p.Reset}
  {p.Option}-s{p.Default},{p.Reset} {p.Option}--skip-dependencies{p.Reset}    {p.Default}Do not run targets' dependencies{p.Reset}
  {p.Option}-v{p.Default},{p.Reset} {p.Option}--verbose{p.Reset}              {p.Default}Enable verbose output{p.Reset}
  {p.Option}--appveyor{p.Reset}                 {p.Default}Force Appveyor mode (normally auto-detected){p.Reset}
  {p.Option}--azure-pipelines{p.Reset}          {p.Default}Force Azure Pipelines mode (normally auto-detected){p.Reset}
  {p.Option}--github-actions{p.Reset}           {p.Default}Force GitHub Actions mode (normally auto-detected){p.Reset}
  {p.Option}--gitlab-ci{p.Reset}                {p.Default}Force GitLab CI mode (normally auto-detected){p.Reset}
  {p.Option}--teamcity{p.Reset}                 {p.Default}Force TeamCity mode (normally auto-detected){p.Reset}
  {p.Option}--travis{p.Reset}                   {p.Default}Force Travis CI mode (normally auto-detected){p.Reset}
  {p.Option}-?{p.Default},{p.Reset} {p.Option}-h{p.Default},{p.Reset} {p.Option}--help{p.Reset}             {p.Default}Show help and usage information, then exit (case insensitive){p.Reset}

{p.Default}Remarks:{p.Reset}
  {p.Default}The {p.Option}--list-xxx{p.Default} options may be combined.{p.Reset}
  {p.Default}The {p.Invocation}invocation{p.Reset} is typically a call to dotnet run, or the path to a script which wraps a call to dotnet run.

{p.Default}Examples:{p.Reset}
  {p.Invocation}./build.{scriptExtension}{p.Reset}
  {p.Invocation}./build.{scriptExtension}{p.Reset} {p.Option}-d{p.Reset}
  {p.Invocation}./build.{scriptExtension}{p.Reset} {p.Option}-t{p.Reset} {p.Option}-i{p.Reset} {p.Target}default{p.Reset}
  {p.Invocation}./build.{scriptExtension}{p.Reset} {p.Target}test{p.Reset} {p.Target}pack{p.Reset}
  {p.Invocation}dotnet run --project targets --{p.Reset} {p.Option}-n{p.Reset} {p.Target}build{p.Reset}

{p.Default}Targets:{p.Reset}
"
            + List(targets, targets.Select(target => target.Name).ToList(), 0, 0, false, "  ");
    }
}
