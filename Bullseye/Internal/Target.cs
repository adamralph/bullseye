using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable RS0016 // Add public types and members to the declared API
namespace Bullseye.Internal
{
    public class Target
    {
        public Target(string name, string description, IEnumerable<string> dependencies)
        {
            this.Name = name ?? throw new InvalidUsageException("Target name cannot be null.");
            this.Description = description;
            this.Dependencies = dependencies.Sanitize().ToList();
        }

        public string Name { get; }

        public string Description { get; }

        public List<string> Dependencies { get; }

        public virtual Task RunAsync(bool dryRun, bool parallel, Logger log, Func<Exception, bool> messageOnly) => log.Succeeded(this.Name);
    }
}
