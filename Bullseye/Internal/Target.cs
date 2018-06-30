namespace Bullseye.Internal
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading.Tasks;

    public class Target
    {
        private readonly Func<Task> action;

        public Target(string name, IEnumerable<string> dependencies, Func<Task> action)
        {
            this.Name = name ?? throw new Exception("A target name cannot be null.");
            this.Dependencies = dependencies.Sanitize().ToList();
            this.action = action;
        }

        public string Name { get; }

        public List<string> Dependencies { get; }

        public async Task RunAsync(bool dryRun, Logger log)
        {
            await log.Starting(this.Name).ConfigureAwait(false);

            var stopWatch = Stopwatch.StartNew();

            if (!dryRun)
            {
                try
                {
                    if (this.action != default)
                    {
                        await this.action().ConfigureAwait(false);
                    }
                }
                catch (Exception ex)
                {
                    await log.Failed(this.Name, ex, stopWatch.Elapsed.TotalMilliseconds).ConfigureAwait(false);
                    throw;
                }
            }

            await log.Succeeded(this.Name, stopWatch.Elapsed.TotalMilliseconds).ConfigureAwait(false);
        }
    }
}
