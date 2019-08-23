namespace Bullseye.Internal
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Threading.Tasks;

    public class ActionTarget : Target
    {
        private readonly Func<Task> _action;

        public ActionTarget(string name, IEnumerable<string> dependencies, Func<Task> action)
            : base(name, dependencies) => _action = action;

        public override async Task RunAsync(bool dryRun, bool parallel, Logger log, Func<Exception, bool> messageOnly)
        {
            await log.Starting(Name).Tax();

            var stopWatch = Stopwatch.StartNew();

            if (!dryRun && _action != default)
            {
                try
                {
                    await _action().Tax();
                }
#pragma warning disable CA1031 // Do not catch general exception types
                catch (Exception ex)
                {
                    if (!messageOnly(ex))
                    {
                        await log.Error(Name, ex).Tax();
                    }

                    await log.Failed(Name, ex, stopWatch.Elapsed.TotalMilliseconds).Tax();
                    throw new TargetFailedException(ex);
                }
#pragma warning restore CA1031 // Do not catch general exception types
            }

            await log.Succeeded(Name, stopWatch.Elapsed.TotalMilliseconds).Tax();
        }
    }
}
