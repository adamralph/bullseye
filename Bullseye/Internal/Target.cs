namespace Bullseye.Internal
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading.Tasks;

    public abstract class Target
    {
        public Target(string name, IEnumerable<string> dependencies)
        {
            this.Name = name ?? throw new Exception("A target name cannot be null.");
            this.Dependencies = dependencies.Sanitize().ToList();
        }

        public string Name { get; }

        public List<string> Dependencies { get; }

        public async Task RunAsync(bool dryRun, Logger log)
        {
            await log.Starting(this.Name).ConfigureAwait(false);

            var stopWatch = Stopwatch.StartNew();

            try
            {
                await this.InvokeAsync(dryRun, log);
            }
            catch (Exception ex)
            {
                await log.Failed(this.Name, ex, stopWatch.Elapsed.TotalMilliseconds).ConfigureAwait(false);
                throw;
            }

            await log.Succeeded(this.Name, stopWatch.Elapsed.TotalMilliseconds).ConfigureAwait(false);
        }

        protected abstract Task InvokeAsync(bool dryRun, Logger log);
    }
}
