using System;

namespace Bullseye.Internal
{
    public class Palette
    {
        public Palette(bool noColor)
        {
            this.Default = noColor ? "" : "\x1b[0m";
            this.Black = noColor ? "" : "\x1b[30m";
            this.Red= noColor ? "" : "\x1b[31m";
            this.Green = noColor ? "" : "\x1b[32m";
            this.Yellow = noColor ? "" : "\x1b[33m";
            this.Magenta = noColor ? "" : "\x1b[35m";
            this.Cyan = noColor ? "" : "\x1b[36m";
            this.White = noColor ? "" : "\x1b[37m";
            this.BrightBlack = noColor ? "" : "\x1b[90m";
            this.BrightRed = noColor ? "" : "\x1b[91m";
            this.BrightGreen = noColor ? "" : "\x1b[92m";
            this.BrightYellow = noColor ? "" : "\x1b[93m";
            this.BrightMagenta = noColor ? "" : "\x1b[95m";
            this.BrightCyan = noColor ? "" : "\x1b[96m";
            this.BrightWhite = noColor ? "" : "\x1b[97m";

            this.Black = Console.BackgroundColor == ConsoleColor.Black ? this.BrightBlack : this.Black;
            this.Red = Console.BackgroundColor == ConsoleColor.DarkRed ? this.BrightRed : this.Red;
            this.Green = Console.BackgroundColor == ConsoleColor.DarkGreen ? this.BrightGreen : this.Green;
            this.Yellow = Console.BackgroundColor == ConsoleColor.DarkYellow ? this.BrightYellow : this.Yellow;
            this.Magenta = Console.BackgroundColor == ConsoleColor.DarkMagenta ? this.BrightMagenta : this.Magenta;
            this.Cyan = Console.BackgroundColor == ConsoleColor.DarkCyan ? this.BrightCyan : this.Cyan;
            this.White = Console.BackgroundColor == ConsoleColor.Gray ? this.BrightWhite : this.White;
            this.BrightBlack = Console.BackgroundColor == ConsoleColor.DarkGray ? this.Black : this.BrightBlack;
            this.BrightRed = Console.BackgroundColor == ConsoleColor.Red ? this.Red : this.BrightRed;
            this.BrightGreen = Console.BackgroundColor == ConsoleColor.Green ? this.Green : this.BrightGreen;
            this.BrightYellow = Console.BackgroundColor == ConsoleColor.Yellow ? this.Yellow : this.BrightYellow;
            this.BrightMagenta = Console.BackgroundColor == ConsoleColor.Magenta ? this.Magenta : this.BrightMagenta;
            this.BrightCyan = Console.BackgroundColor == ConsoleColor.Cyan ? this.Cyan : this.BrightCyan;
            this.BrightWhite = Console.BackgroundColor == ConsoleColor.White ? this.White : this.BrightWhite;
        }

        public string Default { get; }

        public string Black { get; }

        public string Red { get; }

        public string Green { get; }

        public string Yellow { get; }

        public string Magenta { get; }

        public string Cyan { get; }

        public string White { get; }

        public string BrightBlack { get; }

        public string BrightRed { get; }

        public string BrightGreen { get; }

        public string BrightYellow { get; }

        public string BrightMagenta { get; }

        public string BrightCyan { get; }

        public string BrightWhite { get; }
    }
}
