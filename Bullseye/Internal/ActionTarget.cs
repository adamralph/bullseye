using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable RS0016 // Add public types and members to the declared API
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
#pragma warning disable CA1031 // Do not catch general exception types
                catch (Exception ex)
#pragma warning restore CA1031 // Do not catch general exception types
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
