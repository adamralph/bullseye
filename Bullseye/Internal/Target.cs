namespace Bullseye.Internal
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    public class Target
    {
        public Target(IEnumerable<string> dependencies, Func<Task> action)
        {
            this.Dependencies = dependencies.Sanitize().ToList();
            this.Action = action;
        }

        public List<string> Dependencies { get; }

        public Func<Task> Action { get; }
    }
}
