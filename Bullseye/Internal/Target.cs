namespace Bullseye.Internal
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    public class Target
    {
        public Target(string name, IEnumerable<string> dependencies)
        {
            this.Name = name ?? throw new Exception("A target name cannot be null.");
            this.Dependencies = dependencies.Sanitize().ToList();
        }

        public string Name { get; }

        public List<string> Dependencies { get; }

        public virtual Task RunAsync(bool dryRun, bool parallel, Logger log) => log.Succeeded(this.Name, null);
    }
}
