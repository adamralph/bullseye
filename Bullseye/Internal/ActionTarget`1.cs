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

        public override async Task RunAsync(bool dryRun, bool parallel, Logger log)
        {
            var inputsList = this.inputs.ToList();
            if (inputsList.Count == 0)
            {
                await log.NoInputs(this.Name).ConfigureAwait(false);
                return;
            }

            await log.Starting(this.Name).ConfigureAwait(false);
            var stopWatch = Stopwatch.StartNew();

            try
            {
                if (parallel)
                {
                    var tasks = inputsList.Select(input => this.InvokeAsync(input, dryRun, log)).ToList();
                    await Task.WhenAll(tasks).ConfigureAwait(false);
                }
                else
                {
                    foreach (var input in inputsList)
                    {
                        await this.InvokeAsync(input, dryRun, log).ConfigureAwait(false);
                    }
                }
            }

            catch (Exception)
            {
                await log.Failed(this.Name, stopWatch.Elapsed.TotalMilliseconds).ConfigureAwait(false);
                throw;
            }

            await log.Succeeded(this.Name, stopWatch.Elapsed.TotalMilliseconds).ConfigureAwait(false);
        }

        private async Task InvokeAsync(TInput input, bool dryRun, Logger log)
        {
            await log.Starting(this.Name, input).ConfigureAwait(false);
            var stopWatch = Stopwatch.StartNew();

            if (!dryRun)
            {
                try
                {
                    if (this.action != default)
                    {
                        await this.action(input).ConfigureAwait(false);
                    }
                }
                catch (Exception ex)
                {
                    await log.Failed(this.Name, input, ex, stopWatch.Elapsed.TotalMilliseconds).ConfigureAwait(false);
                    throw;
                }
            }

            await log.Succeeded(this.Name, input, stopWatch.Elapsed.TotalMilliseconds).ConfigureAwait(false);
        }
    }
}
