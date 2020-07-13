#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
namespace Bullseye.Internal
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Threading.Tasks;

    public class ActionTarget : Target
    {
        private readonly Func<Task> action;
        private readonly Func<Task> teardown;

        public ActionTarget(string name, IEnumerable<string> dependencies, Func<Task> action, Func<Task> teardown)
            : base(name, dependencies)
        {
            this.action = action;
            this.teardown = teardown;
        }

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

        public override async Task TeardownAsync(bool dryRun, bool parallel, Logger log, Func<Exception, bool> messageOnly, ConcurrentQueue<TargetFailedException> exceptions)
        {
            if (this.teardown == null)
            {
                return;
            }

            //await log.TeardownStarting(this.Name).Tax();

            var stopWatch = Stopwatch.StartNew();

            if (!dryRun)
            {
                try
                {
                    await this.teardown().Tax();
                }
#pragma warning disable CA1031 // Do not catch general exception types
                catch (Exception ex)
#pragma warning restore CA1031 // Do not catch general exception types
                {
                    if (!messageOnly(ex))
                    {
                        //await log.TeardownError(this.Name, ex).Tax();
                    }

                    //await log.TeardownFailed(this.Name, ex, stopWatch.Elapsed).Tax();
                    exceptions.Enqueue(new TargetFailedException($"Target '{this.Name}' failed.", ex));
                }
            }

            //await log.TeardownSucceeded(this.Name, stopWatch.Elapsed).Tax();
        }
    }
}
