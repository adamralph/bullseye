#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
namespace Bullseye.Internal
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    public class Target
    {
        public Target(string name, IEnumerable<string> dependencies, IBuildContext context = default)
        {
            this.Name = name ?? throw new InvalidUsageException("Target name cannot be null.");
            this.Dependencies = dependencies.Sanitize().ToList();
            this.Context = context;
        }

        public string Name { get; }

        public List<string> Dependencies { get; }

        public IBuildContext Context { get; }

        public virtual Task RunAsync(bool dryRun, bool parallel, Logger log, Func<Exception, bool> messageOnly, IBuildContext context) =>
            log.Succeeded(this.Name, null);
    }
}
