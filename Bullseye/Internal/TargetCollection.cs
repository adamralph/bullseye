namespace Bullseye.Internal
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading.Tasks;

    public class TargetCollection : KeyedCollection<string, Target>
    {
        protected override string GetKeyForItem(Target item) => item.Name;

        public async Task RunAsync(List<string> names, bool skipDependencies, bool dryRun, Logger log)
        {
            await log.Running(names).ConfigureAwait(false);
            var stopWatch = Stopwatch.StartNew();

            try
            {
                if (!skipDependencies)
                {
                    this.ValidateDependencies();
                }

                this.Validate(names);

                var targetsRan = new HashSet<string>();
                foreach (var name in names)
                {
                    await this.RunAsync(name, skipDependencies, dryRun, targetsRan, log).ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                await log.Failed(names, ex, stopWatch.Elapsed.TotalMilliseconds).ConfigureAwait(false);
                throw;
            }

            await log.Succeeded(names, stopWatch.Elapsed.TotalMilliseconds).ConfigureAwait(false);
        }

        private async Task RunAsync(string name, bool skipDependencies, bool dryRun, ISet<string> targetsRan, Logger log)
        {
            if (!targetsRan.Add(name))
            {
                return;
            }

            var target = this[name];

            if (!skipDependencies)
            {
                foreach (var dependency in target.Dependencies)
                {
                    await this.RunAsync(dependency, skipDependencies, dryRun, targetsRan, log).ConfigureAwait(false);
                }
            }

            await target.RunAsync(dryRun, log).ConfigureAwait(false);
        }

        private void ValidateDependencies()
        {
            var unknownDependencies = new SortedDictionary<string, SortedSet<string>>();

            foreach (var target in this)
            {
                foreach (var dependency in target.Dependencies
                    .Where(dependency => !this.Contains(dependency)))
                {
                    (unknownDependencies.TryGetValue(dependency, out var set)
                            ? set
                            : unknownDependencies[dependency] = new SortedSet<string>())
                        .Add(target.Name);
                }
            }

            if (!unknownDependencies.Any())
            {
                return;
            }

            var message = $"Missing {(unknownDependencies.Count > 1 ? "dependencies" : "dependency")} detected: " +
                string.Join(
                    "; ",
                    unknownDependencies.Select(missingDependency =>
                        $@"{missingDependency.Key}, required by {missingDependency.Value.Spaced()}"));

            throw new Exception(message);
        }

        private void Validate(List<string> names)
        {
            var unknownNames = new SortedSet<string>(names.Except(this.Select(target => target.Name)));
            if (unknownNames.Any())
            {
                var message = $"The following target{(unknownNames.Count > 1 ? "s were" : " was")} not found: {unknownNames.Spaced()}.";
                throw new Exception(message);
            }
        }
    }
}
