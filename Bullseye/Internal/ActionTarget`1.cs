#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
namespace Bullseye.Internal
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading.Tasks;

    public class ActionTarget<TInput> : Target, IHaveInputs
    {
        private readonly Func<TInput, Task> action;
        private readonly Func<TInput, IBuildContext, Task> actionWithContext;
        private readonly IEnumerable<TInput> inputs;

        public ActionTarget(string name, IEnumerable<string> dependencies, IEnumerable<TInput> inputs, Func<TInput, Task> action)
            : base(name, dependencies)
        {
            this.action = action;
            this.inputs = inputs ?? Enumerable.Empty<TInput>();
        }

        public ActionTarget(string name, IEnumerable<string> dependencies, IEnumerable<TInput> inputs, Func<TInput, IBuildContext, Task> action, IBuildContext context)
            : base(name, dependencies, context)
        {
            this.actionWithContext = action;
            this.inputs = inputs ?? Enumerable.Empty<TInput>();
        }

        public IEnumerable<object> Inputs
        {
            get
            {
                foreach (var input in this.inputs)
                {
                    yield return input;
                }
            }
        }

        public override async Task RunAsync(bool dryRun, bool parallel, Logger log, Func<Exception, bool> messageOnly, IBuildContext context)
        {
            var inputsList = this.inputs.ToList();
            if (inputsList.Count == 0)
            {
                await log.NoInputs(this.Name).Tax();
                return;
            }

            await log.Starting(this.Name).Tax();
            var stopWatch = Stopwatch.StartNew();

            try
            {
                if (parallel)
                {
                    var tasks = inputsList.Select(input => this.InvokeAsync(input, dryRun, log, messageOnly, context)).ToList();
                    await Task.WhenAll(tasks).Tax();
                }
                else
                {
                    foreach (var input in inputsList)
                    {
                        await this.InvokeAsync(input, dryRun, log, messageOnly, context).Tax();
                    }
                }
            }
            catch (Exception)
            {
                await log.Failed(this.Name, stopWatch.Elapsed.TotalMilliseconds).Tax();
                throw;
            }

            await log.Succeeded(this.Name, stopWatch.Elapsed.TotalMilliseconds).Tax();
        }

        private async Task InvokeAsync(TInput input, bool dryRun, Logger log, Func<Exception, bool> messageOnly, IBuildContext context)
        {
            await log.Starting(this.Name, input).Tax();
            var stopWatch = Stopwatch.StartNew();

            if (!dryRun && (this.action != default || this.actionWithContext != default))
            {
                try
                {
                    if (this.actionWithContext is null)
                        await this.action(input).Tax();
                    else
                        await this.actionWithContext(input, this.Context ?? context).Tax();
                }
#pragma warning disable CA1031 // Do not catch general exception types
                catch (Exception ex)
#pragma warning restore CA1031 // Do not catch general exception types
                {
                    if (!messageOnly(ex))
                    {
                        await log.Error(this.Name, input, ex).Tax();
                    }

                    await log.Failed(this.Name, input, ex, stopWatch.Elapsed.TotalMilliseconds).Tax();
                    throw new TargetFailedException(ex);
                }
            }

            await log.Succeeded(this.Name, input, stopWatch.Elapsed.TotalMilliseconds).Tax();
        }
    }
}
