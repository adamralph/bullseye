using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Bullseye.Internal
{
    public class ActionTarget : Target
    {
        private readonly Func<Task> action;

        public ActionTarget(string name, string description, IEnumerable<string> dependencies, Func<Task> action)
            : base(name, description, dependencies) => this.action = action;

        public override async Task RunAsync(bool dryRun, bool parallel, Logger log, Func<Exception, bool> messageOnly)
        {
            await log.Starting(this.Name).Tax();

            TimeSpan? duration = null;

            if (!dryRun && this.action != null)
            {
                try
                {
                    var stopWatch = Stopwatch.StartNew();

                    try
                    {
                        await this.action().Tax();
                    }
                    finally
                    {
                        duration = stopWatch.Elapsed;
                    }
                }
                catch (Exception ex)
                {
                    if (!messageOnly(ex))
                    {
                        await log.Error(this.Name, ex).Tax();
                    }

                    await log.Failed(this.Name, ex, duration).Tax();
                    throw new TargetFailedException($"Target '{this.Name}' failed.", ex);
                }
            }

            await log.Succeeded(this.Name, duration).Tax();
        }
    }
}
