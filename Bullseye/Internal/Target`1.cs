namespace Bullseye.Internal
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading.Tasks;

    public class Target<TInput> : Target, IHaveInputs
    {
        private readonly Func<TInput, Task> action;
        private readonly IEnumerable<TInput> inputs;

        public Target(string name, IEnumerable<string> dependencies, IEnumerable<TInput> inputs, Func<TInput, Task> action)
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

        protected override async Task InvokeAsync(bool dryRun, Logger log)
        {
            if (this.action == default)
            {
                return;
            }

            foreach (var input in this.inputs)
            {
                await log.Starting(this.Name, input).ConfigureAwait(false);
                var stopWatch = Stopwatch.StartNew();

                if (!dryRun)
                {
                    try
                    {
                        await this.action(input).ConfigureAwait(false);
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
}
