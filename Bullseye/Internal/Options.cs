namespace Bullseye.Internal
{
    using System.Collections.Generic;

    public class Options
    {
        public bool Clear { get; set; }

        public bool DryRun { get; set; }

        public bool ListDependencies { get; set; }

        public bool ListInputs { get; set; }

        public bool ListTargets { get; set; }

        public bool ListTree { get; set; }

        public bool NoColor { get; set; }

        public bool Parallel { get; set; }

        public bool SkipDependencies { get; set; }

        public bool Verbose { get; set; }

        public Host Host { get; set; }

        public bool ShowHelp { get; set; }

        public List<string> UnknownOptions { get; } = new List<string>();
    }
}
