namespace Bullseye.Internal
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    public class Target
    {
        public Target(string name, IEnumerable<string> dependencies, Func<Task> action)
        {
            this.Name = name ?? throw new Exception("A target name cannot be null.");
            this.Dependencies = dependencies.Sanitize().ToList();
            this.Action = action;
        }

        public string Name { get; }

        public List<string> Dependencies { get; }

        public Func<Task> Action { get; }
    }
}
