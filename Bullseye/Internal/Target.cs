namespace Bullseye.Internal
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    public class Target
    {
        public Target(string name, IEnumerable<string> dependencies, Body body)
        {
            this.Name = name ?? throw new InvalidUsageException("Target name cannot be null.");
            this.Dependencies = dependencies.Sanitize().ToList();
            this.Bodies.Add(body);
        }

        public string Name { get; }

        public List<string> Dependencies { get; }

        public List<Body> Bodies { get; } = new List<Body>();

        public async Task RunAsync(bool dryRun, bool parallel, Logger log, Func<Exception, bool> messageOnly)
        {
            if (parallel)
            {
                var tasks = this.Bodies.Select(body => body.RunAsync(dryRun, parallel, log, messageOnly));
                await Task.WhenAll(tasks).Tax();
            }
            else
            {
                foreach (var body in this.Bodies)
                {
                    await body.RunAsync(dryRun, parallel, log, messageOnly).Tax();
                }
            }
        }
    }
}
