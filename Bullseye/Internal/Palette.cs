using System;

namespace Bullseye.Internal
{
    public class Palette
    {
        public Palette(bool noColor, bool noExtendedChars, Host host, OperatingSystem operatingSystem)
        {
            var reset = noColor ? "" : "\x1b[0m";

            var black = noColor ? "" : "\x1b[30m";
            var red = noColor ? "" : "\x1b[31m";
            var green = noColor ? "" : "\x1b[32m";
            var yellow = noColor ? "" : "\x1b[33m";
            var blue = noColor ? "" : "\x1b[34m";
            var magenta = noColor ? "" : "\x1b[35m";
            var cyan = noColor ? "" : "\x1b[36m";
            var white = noColor ? "" : "\x1b[37m";

            var brightBlack = noColor ? "" : "\x1b[90m";
            var brightRed = noColor ? "" : "\x1b[91m";
            var brightGreen = noColor ? "" : "\x1b[92m";
            var brightYellow = noColor ? "" : "\x1b[93m";
            var brightBlue = noColor ? "" : "\x1b[94m";
            var brightMagenta = noColor ? "" : "\x1b[95m";
            var brightCyan = noColor ? "" : "\x1b[96m";
            var brightWhite = noColor ? "" : "\x1b[97m";

            black = Console.BackgroundColor == ConsoleColor.Black ? brightBlack : black;
            red = Console.BackgroundColor == ConsoleColor.DarkRed ? brightRed : red;
            green = Console.BackgroundColor == ConsoleColor.DarkGreen ? brightGreen : green;
            yellow = Console.BackgroundColor == ConsoleColor.DarkYellow ? brightYellow : yellow;
            blue = Console.BackgroundColor == ConsoleColor.DarkBlue ? brightBlue : blue;
            magenta = Console.BackgroundColor == ConsoleColor.DarkMagenta ? brightMagenta : magenta;
            cyan = Console.BackgroundColor == ConsoleColor.DarkCyan ? brightCyan : cyan;
            white = Console.BackgroundColor == ConsoleColor.Gray ? brightWhite : white;

            brightBlack = Console.BackgroundColor == ConsoleColor.DarkGray ? black : brightBlack;
            brightRed = Console.BackgroundColor == ConsoleColor.Red ? red : brightRed;
            ////brightGreen = Console.BackgroundColor == ConsoleColor.Green ? green : brightGreen;
            brightYellow = Console.BackgroundColor == ConsoleColor.Yellow ? yellow : brightYellow;
            brightBlue = Console.BackgroundColor == ConsoleColor.Blue ? blue : brightBlue;
            brightMagenta = Console.BackgroundColor == ConsoleColor.Magenta ? magenta : brightMagenta;
            brightCyan = Console.BackgroundColor == ConsoleColor.Cyan ? cyan : brightCyan;
            ////brightWhite = Console.BackgroundColor == ConsoleColor.White ? white : brightWhite;

            this.Invocation = brightYellow;
            this.Default = white;
            this.Failed = brightRed;
            this.Input = brightCyan;
            this.Option = brightMagenta;
            this.Prefix = brightBlack;
            this.Reset = reset;
            this.Succeeded = green;
            this.Target = cyan;
            this.Timing = magenta;
            this.Verbose = brightBlack;
            this.Warning = brightYellow;

            this.TreeCorner = $"{green}└─{reset}";
            this.TreeFork = $"{green}├─{reset}";
            this.TreeDown = $"{green}│{reset} ";
            this.Dash = '─';

            if (host == Host.Appveyor &&
                (operatingSystem == OperatingSystem.Windows || operatingSystem == OperatingSystem.Linux))
            {
                this.Default = brightBlack;
                this.Target = blue;

                if (operatingSystem == OperatingSystem.Windows)
                {
                    this.TreeCorner = "  ";
                    this.TreeFork = "  ";
                    this.TreeDown = "  ";
                    this.Dash = '-';
                }

                if (operatingSystem == OperatingSystem.Linux)
                {
                    this.Timing = brightMagenta;
                }
            }

            if (host == Host.Travis || host == Host.AzurePipelines)
            {
                this.Invocation = yellow;
                this.Default = brightBlack;
                this.Failed = red;
                this.Input = cyan;
                this.Option = magenta;
                this.Target = blue;
                this.Warning = yellow;
            }

            if (host == Host.TeamCity)
            {
                this.Default = brightBlack;
                this.Target = brightBlue;
                this.TreeCorner = "  ";
                this.TreeFork = "  ";
                this.TreeDown = "  ";
                this.Dash = '-';
            }

            if (host == Host.GitHubActions)
            {
                this.Invocation = yellow;
                this.Failed = red;
                this.Target = blue;
            }

            if (host == Host.GitLabCI)
            {
                this.Target = blue;
            }

            if (host == Host.VisualStudioCode)
            {
                this.Target = blue;
            }

            if (noExtendedChars)
            {
                this.TreeCorner = "  ";
                this.TreeFork = "  ";
                this.TreeDown = "  ";
                this.Dash = '-';
            }
        }

        public string Invocation { get; }

        public string Reset { get; }

        public string Failed { get; }

        public string Input { get; }

        public string Option { get; }

        public string Prefix { get; }

        public string Succeeded { get; }

        public string Target { get; }

        public string Default { get; }

        public string Timing { get; }

        public string Verbose { get; }

        public string Warning { get; }

        public string TreeCorner { get; }

        public string TreeFork { get; }

        public string TreeDown { get; }

        public char Dash { get; }

        public static string StripColours(string text)
        {
            foreach (var number in new[] { 0, 30, 31, 32, 33, 34, 35, 36, 37, 90, 91, 92, 93, 94, 95, 96, 97 })
            {
                text = text.Replace($"\x1b[{number}m", "");
            }

            return text;
        }
    }
}
