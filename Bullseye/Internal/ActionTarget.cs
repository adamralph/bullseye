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

        public override async Task RunAsync(bool dryRun, bool parallel, Output output, Func<Exception, bool> messageOnly, IEnumerable<Target> dependencyPath)
        {
            await output.Starting(this, dependencyPath).Tax();

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
                        await output.Error(this, ex).Tax();
                    }

                    await output.Failed(this, ex, duration, dependencyPath).Tax();

                    throw new TargetFailedException($"Target '{this.Name}' failed.", ex);
                }
            }

            await output.Succeeded(this, duration, dependencyPath).Tax();
        }
    }
}
