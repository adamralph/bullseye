namespace Bullseye.Internal
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Threading.Tasks;

    public class ActionTarget : Target
    {
        private readonly Func<Task> action;

        public ActionTarget(string name, IEnumerable<string> dependencies, Func<Task> action)
            : base(name, dependencies) => this.action = action;

        public override async Task RunAsync(bool dryRun, bool parallel, Logger log, Func<Exception, bool> messageOnly)
        {
            await log.Starting(this.Name).Tax();

            var stopWatch = Stopwatch.StartNew();

            if (!dryRun && this.action != default)
            {
                try
                {
                    await this.action().Tax();
                }
#pragma warning disable CA1031 // Do not catch general exception types
                catch (Exception ex)
                {
                    if (!messageOnly(ex))
                    {
                        await log.Error(this.Name, ex).Tax();
                    }

                    await log.Failed(this.Name, ex, stopWatch.Elapsed.TotalMilliseconds).Tax();
                    throw new TargetFailedException(ex);
                }
#pragma warning restore CA1031 // Do not catch general exception types
            }

            await log.Succeeded(this.Name, stopWatch.Elapsed.TotalMilliseconds).Tax();
        }
    }
}
