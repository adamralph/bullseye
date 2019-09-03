#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
namespace Bullseye.Internal
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Threading.Tasks;

    public class ActionTarget : Target
    {
        private readonly Func<Task> action;
        private readonly Func<IBuildContext, Task> actionWithContext;

        public ActionTarget(string name, IEnumerable<string> dependencies, Func<Task> action)
            : base(name, dependencies) => this.action = action;

        public ActionTarget(string name, IEnumerable<string> dependencies, Func<IBuildContext, Task> action, IBuildContext context)
            : base(name, dependencies, context) => this.actionWithContext = action;

        public override async Task RunAsync(bool dryRun, bool parallel, Logger log, Func<Exception, bool> messageOnly, IBuildContext context)
        {
            await log.Starting(this.Name).Tax();

            var stopWatch = Stopwatch.StartNew();

            if (!dryRun && (this.action != default || this.actionWithContext != default))
            {
                try
                {
                    if (this.actionWithContext is null)
                        await this.action().Tax();
                    else
                        await this.actionWithContext(this.Context ?? context).Tax();
                }
#pragma warning disable CA1031 // Do not catch general exception types
                catch (Exception ex)
#pragma warning restore CA1031 // Do not catch general exception types
                {
                    if (!messageOnly(ex))
                    {
                        await log.Error(this.Name, ex).Tax();
                    }

                    await log.Failed(this.Name, ex, stopWatch.Elapsed.TotalMilliseconds).Tax();
                    throw new TargetFailedException(ex);
                }
            }

            await log.Succeeded(this.Name, stopWatch.Elapsed.TotalMilliseconds).Tax();
        }
    }
}
