using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Bullseye.Internal
{
    public class ActionTarget<TInput> : Target, IHaveInputs
    {
        private readonly Func<TInput, Task> action;
        private readonly IEnumerable<TInput> inputs;

        public ActionTarget(string name, string description, IEnumerable<string> dependencies, IEnumerable<TInput> inputs, Func<TInput, Task> action)
            : base(name, description, dependencies)
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

        public override async Task RunAsync(bool dryRun, bool parallel, Output output, Func<Exception, bool> messageOnly, IReadOnlyCollection<Target> dependencyPath)
        {
            var inputsList = this.inputs.ToList();

            if (inputsList.Count == 0)
            {
                await output.NoInputs(this, dependencyPath).Tax();
                return;
            }

            await output.Starting(this, dependencyPath).Tax();

            try
            {
                if (parallel)
                {
                    var tasks = inputsList.Select(input => this.RunAsync(input, dryRun, output, messageOnly, dependencyPath)).ToList();

                    await Task.WhenAll(tasks).Tax();
                }
                else
                {
                    foreach (var input in inputsList)
                    {
                        await this.RunAsync(input, dryRun, output, messageOnly, dependencyPath).Tax();
                    }
                }
            }
            catch (Exception)
            {
                await output.Failed(this, dependencyPath).Tax();
                throw;
            }

            await output.Succeeded(this, dependencyPath).Tax();
        }

        private async Task RunAsync(TInput input, bool dryRun, Output output, Func<Exception, bool> messageOnly, IReadOnlyCollection<Target> dependencyPath)
        {
            var id = Guid.NewGuid();

            await output.Starting(this, input, id, dependencyPath).Tax();

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
                catch (Exception ex)
                {
                    if (!messageOnly(ex))
                    {
                        await output.Error(this, input, ex).Tax();
                    }

                    await output.Failed(this, input, ex, duration, id, dependencyPath).Tax();

                    throw new TargetFailedException($"Target '{this.Name}' failed with input '{input}'.", ex);
                }
            }

            await output.Succeeded(this, input, duration, id, dependencyPath).Tax();
        }
    }
}
