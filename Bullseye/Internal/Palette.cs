namespace Bullseye.Internal
{
    public class Palette
    {
        public Palette(bool noColor)
        {
            this.Default = noColor ? "" : "\x1b[0m";
            this.Green = noColor ? "" : "\x1b[32m";
            this.Magenta = noColor ? "" : "\x1b[35m";
            this.Cyan = noColor ? "" : "\x1b[36m";
            this.White = noColor ? "" : "\x1b[37m";
            this.BrightRed = noColor ? "" : "\x1b[91m";
            this.BrightYellow = noColor ? "" : "\x1b[93m";
            this.BrightMagenta = noColor ? "" : "\x1b[95m";
        }

        public string Default { get; }

        public string Green { get; }

        public string Magenta { get; }

        public string Cyan { get; }

        public string White { get; }

        public string BrightRed { get; }

        public string BrightYellow { get; }

        public string BrightMagenta { get; }
    }
}
