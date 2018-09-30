using System;
using System.Runtime.InteropServices;

namespace Bullseye.Internal
{
    public class Palette
    {
        private readonly string @default;

        private readonly string black;
        private readonly string red;
        private readonly string green;
        private readonly string yellow;
        private readonly string blue;
        private readonly string magenta;
        private readonly string cyan;
        private readonly string white;

        private readonly string brightBlack;
        private readonly string brightRed;
        private readonly string brightGreen;
        private readonly string brightYellow;
        private readonly string brightBlue;
        private readonly string brightMagenta;
        private readonly string brightCyan;
        private readonly string brightWhite;

        public Palette(bool noColor)
        {
            this.@default = noColor ? "" : "\x1b[0m";

            this.black = noColor ? "" : "\x1b[30m";
            this.red = noColor ? "" : "\x1b[31m";
            this.green = noColor ? "" : "\x1b[32m";
            this.yellow = noColor ? "" : "\x1b[33m";
            this.blue = noColor ? "" : "\x1b[34m";
            this.magenta = noColor ? "" : "\x1b[35m";
            this.cyan = noColor ? "" : "\x1b[36m";
            this.white = noColor ? "" : "\x1b[37m";

            this.brightBlack = noColor ? "" : "\x1b[90m";
            this.brightRed = noColor ? "" : "\x1b[91m";
            this.brightGreen = noColor ? "" : "\x1b[92m";
            this.brightYellow = noColor ? "" : "\x1b[93m";
            this.brightBlue = noColor ? "" : "\x1b[94m";
            this.brightMagenta = noColor ? "" : "\x1b[95m";
            this.brightCyan = noColor ? "" : "\x1b[96m";
            this.brightWhite = noColor ? "" : "\x1b[97m";

            this.black = Console.BackgroundColor == ConsoleColor.Black ? this.brightBlack : this.black;
            this.red = Console.BackgroundColor == ConsoleColor.DarkRed ? this.brightRed : this.red;
            this.green = Console.BackgroundColor == ConsoleColor.DarkGreen ? this.brightGreen : this.green;
            this.yellow = Console.BackgroundColor == ConsoleColor.DarkYellow ? this.brightYellow : this.yellow;
            this.blue = Console.BackgroundColor == ConsoleColor.DarkBlue ? this.brightBlue : this.blue;
            this.magenta = Console.BackgroundColor == ConsoleColor.DarkMagenta ? this.brightMagenta : this.magenta;
            this.cyan = Console.BackgroundColor == ConsoleColor.DarkCyan ? this.brightCyan : this.cyan;
            this.white = Console.BackgroundColor == ConsoleColor.Gray ? this.brightWhite : this.white;

            this.brightBlack = Console.BackgroundColor == ConsoleColor.DarkGray ? this.black : this.brightBlack;
            this.brightRed = Console.BackgroundColor == ConsoleColor.Red ? this.red : this.brightRed;
            this.brightGreen = Console.BackgroundColor == ConsoleColor.Green ? this.green : this.brightGreen;
            this.brightYellow = Console.BackgroundColor == ConsoleColor.Yellow ? this.yellow : this.brightYellow;
            this.brightBlue = Console.BackgroundColor == ConsoleColor.Blue ? this.blue : this.brightBlue;
            this.brightMagenta = Console.BackgroundColor == ConsoleColor.Magenta ? this.magenta : this.brightMagenta;
            this.brightCyan = Console.BackgroundColor == ConsoleColor.Cyan ? this.cyan : this.brightCyan;
            this.brightWhite = Console.BackgroundColor == ConsoleColor.White ? this.white : this.brightWhite;

            this.CommandLine = this.brightYellow;
            this.Default = this.@default;
            this.Dependency = this.white;
            this.Failed = this.brightRed;
            this.Input = this.brightCyan;
            this.Label = this.cyan;
            this.Option = this.brightMagenta;
            this.Starting = this.white;
            this.Succeeded = this.green;
            this.Symbol = this.white;
            this.Target = this.brightWhite;
            this.Text = this.white;
            this.Timing = this.magenta;
            this.Warning = this.brightYellow;

            if (Environment.GetEnvironmentVariable("APPVEYOR")?.ToUpperInvariant() == "TRUE" &&
                (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ||
                RuntimeInformation.IsOSPlatform(OSPlatform.Linux)))
            {
                this.Dependency = this.brightBlack;
                this.Label = this.brightBlue;
                this.Starting = this.brightBlack;
                this.Symbol = this.brightBlack;
                this.Text = this.brightBlack;

                if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    this.Timing = this.brightMagenta;
                }
            }

            var travisOSName = Environment.GetEnvironmentVariable("TRAVIS_OS_NAME");
            if (travisOSName == "linux" || travisOSName == "osx")
            {
                this.CommandLine = this.yellow;
                this.Dependency = this.brightBlack;
                this.Failed = this.red;
                this.Input = this.cyan;
                this.Label = this.blue;
                this.Option = this.magenta;
                this.Starting = this.brightBlack;
                this.Symbol = this.brightBlack;
                this.Target = this.white;
                this.Text = this.brightBlack;
                this.Warning = this.yellow;
            }

            if (!string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("TEAMCITY_PROJECT_NAME")))
            {
                this.Dependency = this.brightBlack;
                this.Label = this.brightBlue;
                this.Starting = this.brightBlack;
                this.Symbol = this.brightBlack;
                this.Text = this.brightBlack;
            }
        }

        public string CommandLine { get; }

        public string Default { get; }

        public string Dependency { get; }

        public string Failed { get; }

        public string Input { get; }

        public string Label { get; }

        public string Option { get; }

        public string Starting { get; }

        public string Succeeded { get; }

        public string Symbol { get; }

        public string Target { get; }

        public string Text { get; }

        public string Timing { get; }

        public string Warning { get; }
    }
}
