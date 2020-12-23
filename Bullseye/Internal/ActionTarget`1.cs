using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable RS0016 // Add public types and members to the declared API
namespace Bullseye.Internal
{
    public class ActionTarget<TInput> : Target, IHaveInputs
    {
        private readonly Func<TInput, Task> action;
        private readonly IEnumerable<TInput> inputs;

        public ActionTarget(string name, IEnumerable<string> dependencies, IEnumerable<TInput> inputs, Func<TInput, Task> action)
            : base(name, dependencies)
        {
            this.action = action;
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

        public override async Task RunAsync(bool dryRun, bool parallel, Logger log, Func<Exception, bool> messageOnly)
        {
            var inputsList = this.inputs.ToList();
            if (inputsList.Count == 0)
            {
                await log.NoInputs(this.Name).Tax();
                return;
            }

            await log.Starting(this.Name).Tax();

            try
            {
                if (parallel)
                {
                    var tasks = inputsList.Select(input => this.RunAsync(input, dryRun, log, messageOnly)).ToList();
                    await Task.WhenAll(tasks).Tax();
                }
                else
                {
                    foreach (var input in inputsList)
                    {
                        await this.RunAsync(input, dryRun, log, messageOnly).Tax();
                    }
                }
            }
            catch (Exception)
            {
                await log.Failed(this.Name).Tax();
                throw;
            }

            await log.Succeeded(this.Name).Tax();
        }

        private async Task RunAsync(TInput input, bool dryRun, Logger log, Func<Exception, bool> messageOnly)
        {
            var id = Guid.NewGuid();
            await log.Starting(this.Name, input, id).Tax();
            TimeSpan? duration = null;

            if (!dryRun && this.action != null)
            {
                try
                {
                    var stopWatch = Stopwatch.StartNew();

                    try
                    {
                        await this.action(input).Tax();
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
                        await log.Error(this.Name, input, ex).Tax();
                    }

                    await log.Failed(this.Name, input, ex, duration, id).Tax();
                    throw new TargetFailedException($"Target '{this.Name}' failed with input '{input}'.", ex);
                }
            }

            await log.Succeeded(this.Name, input, duration, id).Tax();
        }
    }
}
