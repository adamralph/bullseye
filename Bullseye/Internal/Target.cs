namespace Bullseye.Internal
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public class Target
    {
        public Target(List<string> dependencies, Func<Task> action)
        {
            this.Dependencies = dependencies;
            this.Action = action;
        }

        public List<string> Dependencies { get; }

        public Func<Task> Action { get; }
    }
}
