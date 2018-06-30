namespace Bullseye.Internal
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public class TargetWithoutInput : Target
    {
        private readonly Func<Task> action;

        public TargetWithoutInput(string name, IEnumerable<string> dependencies, Func<Task> action)
            : base(name, dependencies) => this.action = action;

        protected override Task InvokeAsync(bool dryRun, Logger log) => !dryRun && this.action != default
            ? this.action()
            : Task.CompletedTask;
    }
}
