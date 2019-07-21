namespace Bullseye.Internal
{
    using System;
    using System.Threading.Tasks;

    public class Body
    {
        public Body(string name) => this.Name = name;

        public string Name { get; }

        public virtual Task RunAsync(bool dryRun, bool parallel, Logger log, Func<Exception, bool> messageOnly) => log.Succeeded(this.Name, null);
    }
}
