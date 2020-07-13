#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
namespace Bullseye.Internal
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading.Tasks;

    public class ActionTarget<TInput> : Target, IHaveInputs
    {
        private readonly Func<TInput, Task> action;
        private readonly Func<TInput, Task> teardown;
        private readonly IEnumerable<TInput> inputs;
        private readonly ConcurrentStack<TInput> teardownInputs = new ConcurrentStack<TInput>();

        public ActionTarget(string name, IEnumerable<string> dependencies, IEnumerable<TInput> inputs, Func<TInput, Task> action, Func<TInput, Task> teardown)
            : base(name, dependencies)
        {
            this.action = action;
            this.teardown = teardown;
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

        public override async Task TeardownAsync(bool dryRun, bool parallel, Logger log, Func<Exception, bool> messageOnly, ConcurrentQueue<TargetFailedException> exceptions)
        {
            if (this.teardown == null || !this.teardownInputs.Any())
            {
                return;
            }

            var inputsList = new TInput[this.teardownInputs.Count];
            this.teardownInputs.TryPopRange(inputsList);

            //await log.TeardownStarting(this.Name).Tax();
            var stopWatch = Stopwatch.StartNew();

            Queue<TargetFailedException> myExceptions;
            if (parallel)
            {
                var tasks = inputsList.Select(input => this.TeardownAsync(input, dryRun, log, messageOnly)).ToList();
                myExceptions = new Queue<TargetFailedException>(await Task.WhenAll(tasks).Tax());
            }
            else
            {
                myExceptions = new Queue<TargetFailedException>();
                foreach (var input in inputsList)
                {
                    myExceptions.Enqueue(await this.TeardownAsync(input, dryRun, log, messageOnly).Tax());
                }
            }

            myExceptions = new Queue<TargetFailedException>(myExceptions.Where(ex => ex != null));

            if (myExceptions.Any())
            {
                //await log.TeardownFailed(this.Name, stopWatch.Elapsed).Tax();
            }
            else
            {
                //await log.TeardownSucceeded(this.Name, stopWatch.Elapsed).Tax();
            }

            foreach (var ex in myExceptions)
            {
                exceptions.Enqueue(ex);
            }
        }

        private async Task<TargetFailedException> TeardownAsync(TInput input, bool dryRun, Logger log, Func<Exception, bool> messageOnly)
        {
            var id = Guid.NewGuid();
            //await log.TeardownStarting(this.Name, input, id).Tax();
            var stopWatch = Stopwatch.StartNew();

            if (!dryRun)
            {
                try
                {
                    await this.teardown(input).Tax();
                }
#pragma warning disable CA1031 // Do not catch general exception types
                catch (Exception ex)
#pragma warning restore CA1031 // Do not catch general exception types
                {
                    if (!messageOnly(ex))
                    {
                        //await log.TeardownError(this.Name, input, ex).Tax();
                    }

                    //await log.TeardownFailed(this.Name, input, ex, stopWatch.Elapsed, id).Tax();
                    return new TargetFailedException($"Target '{this.Name}' failed with input '{input}'.", ex);
                }
                finally
                {
                    this.teardownInputs.Push(input);
                }
            }

            //await log.TeardownSucceeded(this.Name, input, stopWatch.Elapsed, id).Tax();
            return null;
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
                finally
                {
                    this.teardownInputs.Push(input);
                }
            }

            await log.Succeeded(this.Name, input, duration, id).Tax();
        }
    }
}
