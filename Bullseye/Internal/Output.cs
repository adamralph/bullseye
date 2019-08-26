#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable IDE0009 // Member access should be qualified.
namespace Bullseye.Internal
{
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public class Output
    {
        private readonly TextWriter writer;
        private readonly Palette p;

        public Output(TextWriter writer, Palette palette)
        {
            this.writer = writer;
            this.p = palette;
        }

        public Task Usage(TargetCollection targets) => this.writer.WriteLineAsync(this.GetUsage(targets));

        public Task Targets(TargetCollection targets, List<string> rootTargets, int maxDepth, int maxDepthToShowInputs, bool listInputs) =>
            this.writer.WriteLineAsync(this.List(targets, rootTargets, maxDepth, maxDepthToShowInputs, listInputs));

        private string List(TargetCollection targets, List<string> rootTargets, int maxDepth, int maxDepthToShowInputs, bool listInputs)
        {
            var value = new StringBuilder();

            foreach (var rootTarget in rootTargets)
            {
                Append(new List<string> { rootTarget }, new Stack<string>(), true, "", 0);
            }

            return value.ToString();

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
                            ? ""
                            : $"{previousPrefix.Replace(p.TreeCorner, "  ").Replace(p.TreeFork, p.TreeDown)}{(item.index == names.Count - 1 ? p.TreeCorner : p.TreeFork)}";

                        var isMissing = !targets.Contains(item.name);

                        value.Append($"{p.Tree}{prefix}{p.Target}{item.name}{p.Default}");

                        if (isMissing)
                        {
                            value.AppendLine($" {p.Failed}(missing){p.Default}");
                            continue;
                        }

                        if (circularDependency)
                        {
                            value.AppendLine($" {p.Failed}(circular dependency){p.Default}");
                            continue;
                        }

                        value.AppendLine(p.Default);

                        var target = targets[item.name];

                        if (listInputs && depth <= maxDepthToShowInputs && target is IHaveInputs hasInputs)
                        {
                            foreach (var inputItem in hasInputs.Inputs.Select((input, index) => new { input, index }))
                            {
                                var inputPrefix = $"{prefix.Replace(p.TreeCorner, "  ").Replace(p.TreeFork, p.TreeDown)}{(target.Dependencies.Any() && depth + 1 <= maxDepth ? p.TreeDown : "  ")}";

                                value.AppendLine($"{p.Tree}{inputPrefix}{p.Input}{inputItem.input}{p.Default}");
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

        private string GetUsage(TargetCollection targets) =>
$@"{p.Text}Usage: {p.CommandLine}<command-line> {p.Option}[<options>] {p.Target}[<targets>]

{p.Text}command-line: {p.Text}The command line which invokes the build targets.
  {p.Text}Examples:
    {p.CommandLine}build.cmd
    {p.CommandLine}build.sh
    {p.CommandLine}dotnet run --project targets --

{p.Text}options:
 {p.Option}-c, --clear                {p.Text}Clear the console before execution
 {p.Option}-n, --dry-run              {p.Text}Do a dry run without executing actions
 {p.Option}-d, --list-dependencies    {p.Text}List all (or specified) targets and dependencies, then exit
 {p.Option}-i, --list-inputs          {p.Text}List all (or specified) targets and inputs, then exit
 {p.Option}-l, --list-targets         {p.Text}List all (or specified) targets, then exit
 {p.Option}-t, --list-tree            {p.Text}List all (or specified) targets and dependency trees, then exit
 {p.Option}-N, --no-color             {p.Text}Disable colored output
 {p.Option}-p, --parallel             {p.Text}Run targets in parallel
 {p.Option}-s, --skip-dependencies    {p.Text}Do not run targets' dependencies
 {p.Option}-v, --verbose              {p.Text}Enable verbose output
 {p.Option}    --appveyor             {p.Text}Force Appveyor mode (normally auto-detected)
 {p.Option}    --azure-pipelines      {p.Text}Force Azure Pipelines mode (normally auto-detected)
 {p.Option}    --github-actions       {p.Text}Force GitHub Actions mode (normally auto-detected)
 {p.Option}    --gitlab-ci            {p.Text}Force GitLab CI mode (normally auto-detected)
 {p.Option}    --teamcity             {p.Text}Force TeamCity mode (normally auto-detected)
 {p.Option}    --travis               {p.Text}Force Travis CI mode (normally auto-detected)
 {p.Option}-h, --help, -?             {p.Text}Show this help, then exit (case insensitive)

{p.Text}targets: {p.Text}A list of targets to run or list.
  If not specified, the {p.Target}""default""{p.Text} target will be run, or all targets will be listed.

{p.Text}Remarks:
  {p.Text}The {p.Option}--list-xxx {p.Text}options can be combined.

{p.Text}Examples:
  {p.CommandLine}build.cmd
  {p.CommandLine}build.cmd {p.Option}-D
  {p.CommandLine}build.sh {p.Option}-t -I {p.Target}default
  {p.CommandLine}build.sh {p.Target}test pack
  {p.CommandLine}dotnet run --project targets -- {p.Option}-n {p.Target}build{p.Default}

{p.Text}Targets:
"
            + string.Join(
@"
",
                targets.Select(target => $"  {p.Target}{target.Name}{p.Default}"));
    }
}
