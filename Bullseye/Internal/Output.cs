#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable IDE0009 // Member access should be qualified.
#pragma warning disable RS0016 // Add public types and members to the declared API
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

        public Task Usage(TargetCollection targets) => this.writer.WriteAsync(this.GetUsage(targets));

        public Task Targets(TargetCollection targets, List<string> rootTargets, int maxDepth, int maxDepthToShowInputs, bool listInputs) =>
            this.writer.WriteAsync(this.List(targets, rootTargets, maxDepth, maxDepthToShowInputs, listInputs));

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

                        value.Append($"{prefix}{p.Target}{item.name}");

                        if (isMissing)
                        {
                            value.AppendLine($"{p.Reset} {p.Failed}(missing){p.Reset}");
                            continue;
                        }

                        if (circularDependency)
                        {
                            value.AppendLine($"{p.Reset} {p.Failed}(circular dependency){p.Reset}");
                            continue;
                        }

                        value.AppendLine(p.Reset);

                        var target = targets[item.name];

                        if (listInputs && depth <= maxDepthToShowInputs && target is IHaveInputs hasInputs)
                        {
                            foreach (var inputItem in hasInputs.Inputs.Select((input, index) => new { input, index }))
                            {
                                var inputPrefix = $"{prefix.Replace(p.TreeCorner, "  ").Replace(p.TreeFork, p.TreeDown)}{(target.Dependencies.Any() && depth + 1 <= maxDepth ? p.TreeDown : "  ")}";

                                value.AppendLine($"{inputPrefix}{p.Input}{inputItem.input}{p.Reset}");
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
$@"{p.Default}Usage:{p.Reset} {p.CommandLine}<command-line>{p.Reset} {p.Option}[<options>]{p.Reset} {p.Target}[<targets>]{p.Reset}

{p.Default}command-line: The command line which invokes the build targets.{p.Reset}
  {p.Default}Examples:{p.Reset}
    {p.CommandLine}build.cmd{p.Reset}
    {p.CommandLine}build.sh{p.Reset}
    {p.CommandLine}dotnet run --project targets --{p.Reset}

{p.Default}options:{p.Reset}
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
     {p.Option}--appveyor{p.Reset}             {p.Default}Force Appveyor mode (normally auto-detected){p.Reset}
     {p.Option}--azure-pipelines{p.Reset}      {p.Default}Force Azure Pipelines mode (normally auto-detected){p.Reset}
     {p.Option}--github-actions{p.Reset}       {p.Default}Force GitHub Actions mode (normally auto-detected){p.Reset}
     {p.Option}--gitlab-ci{p.Reset}            {p.Default}Force GitLab CI mode (normally auto-detected){p.Reset}
     {p.Option}--teamcity{p.Reset}             {p.Default}Force TeamCity mode (normally auto-detected){p.Reset}
     {p.Option}--travis{p.Reset}               {p.Default}Force Travis CI mode (normally auto-detected){p.Reset}
 {p.Option}-h{p.Default},{p.Reset} {p.Option}--help{p.Default},{p.Reset} {p.Option}-?{p.Reset}             {p.Default}Show this help, then exit (case insensitive){p.Reset}

{p.Default}targets: A list of targets to run or list.{p.Reset}
  {p.Default}If not specified, the {p.Target}""default""{p.Default} target will be run, or all targets will be listed.{p.Reset}

{p.Default}Remarks:{p.Reset}
  {p.Default}The {p.Option}--list-xxx{p.Default} options can be combined.{p.Reset}

{p.Default}Examples:{p.Reset}
  {p.CommandLine}build.cmd{p.Reset}
  {p.CommandLine}build.cmd{p.Reset} {p.Option}-D{p.Reset}
  {p.CommandLine}build.sh{p.Reset} {p.Option}-t{p.Reset} {p.Option}-I{p.Reset} {p.Target}default{p.Reset}
  {p.CommandLine}build.sh{p.Reset} {p.Target}test{p.Reset} {p.Target}pack{p.Reset}
  {p.CommandLine}dotnet run --project targets --{p.Reset} {p.Option}-n{p.Reset} {p.Target}build{p.Reset}

{p.Default}Targets:{p.Reset}
"
            + string.Join(
@"
",
                targets.Select(target => $"  {p.Target}{target.Name}{p.Reset}"))
            +
@"
";
    }
}
