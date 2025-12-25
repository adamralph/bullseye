using System.Runtime.InteropServices;

namespace Bullseye;

/// <summary>
/// A palette of strings representing color and other characters.
/// </summary>
public class Palette
{
    private static readonly int[] Numbers = [0, 30, 31, 32, 33, 34, 35, 36, 37, 90, 91, 92, 93, 94, 95, 96, 97,];

    /// <summary>
    /// Constructs an instance of <see cref="Palette"/>.
    /// </summary>
    /// <param name="noColor">Whether to use color or not.</param>
    /// <param name="noExtendedChars">Whether to use extended characters or not.</param>
    /// <param name="host">The host environment.</param>
    /// <param name="osPlatform">The OS platform.</param>
    public Palette(bool noColor, bool noExtendedChars, Host host, OSPlatform osPlatform)
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

        Invocation = brightYellow;
        Text = white;
        Failure = brightRed;
        Input = brightCyan;
        Option = brightMagenta;
        Prefix = brightBlack;
        Default = reset;
        Success = green;
        Target = cyan;
        Timing = magenta;
        Verbose = brightBlack;
        Warning = brightYellow;

        TreeCorner = $"{green}└─{reset}";
        TreeFork = $"{green}├─{reset}";
        TreeLine = $"{green}│{reset} ";
        Horizontal = '─';

#pragma warning disable IDE0010 // Add missing cases to switch statement
        switch (host)
        {
            case Host.AppVeyor when osPlatform == OSPlatform.Windows:
                Text = brightBlack;
                Target = blue;
                TreeCorner = "  ";
                TreeFork = "  ";
                TreeLine = "  ";
                Horizontal = '-';
                break;
            case Host.AppVeyor when osPlatform == OSPlatform.Linux:
                Text = brightBlack;
                Target = blue;
                Timing = brightMagenta;
                break;
            case Host.GitHubActions:
                Invocation = yellow;
                Failure = red;
                Target = blue;
                break;
            case Host.GitLabCI:
                Target = blue;
                break;
            case Host.TeamCity:
                Text = brightBlack;
                Target = brightBlue;
                TreeCorner = "  ";
                TreeFork = "  ";
                TreeLine = "  ";
                Horizontal = '-';
                break;
            case Host.Travis:
                Invocation = yellow;
                Text = brightBlack;
                Failure = red;
                Input = cyan;
                Option = magenta;
                Target = blue;
                Warning = yellow;
                break;
            case Host.VisualStudioCode:
                Target = blue;
                break;
        }
#pragma warning restore IDE0010 // Add missing cases to switch statement

        if (noExtendedChars)
        {
            TreeCorner = "  ";
            TreeFork = "  ";
            TreeLine = "  ";
            Horizontal = '-';
        }
    }

    /// <summary>
    /// The color to use for a command invocation.
    /// </summary>
    public string Invocation { get; }

    /// <summary>
    /// The default color.
    /// </summary>
    public string Default { get; }

    /// <summary>
    /// The color to use for a failure.
    /// </summary>
    public string Failure { get; }

    /// <summary>
    /// The color to use for an input.
    /// </summary>
    public string Input { get; }

    /// <summary>
    /// The color to use for a command option.
    /// </summary>
    public string Option { get; }

    /// <summary>
    /// The color to use for a program prefix.
    /// </summary>
    public string Prefix { get; }

    /// <summary>
    /// The color to use for success.
    /// </summary>
    public string Success { get; }

    /// <summary>
    /// The color to use for a target.
    /// </summary>
    public string Target { get; }

    /// <summary>
    /// The color to use for arbitrary text.
    /// </summary>
    public string Text { get; }

    /// <summary>
    /// The color to use for a timing.
    /// </summary>
    public string Timing { get; }

    /// <summary>
    /// The color to use for a verbose message.
    /// </summary>
    public string Verbose { get; }

    /// <summary>
    /// The color to use for a warning.
    /// </summary>
    public string Warning { get; }

    /// <summary>
    /// The string to use for a tree corner.
    /// </summary>
    public string TreeCorner { get; }

    /// <summary>
    /// The string to use for a tree fork.
    /// </summary>
    public string TreeFork { get; }

    /// <summary>
    /// The string to use for a tree line.
    /// </summary>
    public string TreeLine { get; }

    /// <summary>
    /// The string to use for a horizontal line.
    /// </summary>
    public char Horizontal { get; }

    /// <summary>
    /// Strip colors from a specified <see cref="string"/>.
    /// </summary>
    /// <param name="text">The <see cref="string"/> from which to strip colors.</param>
    /// <returns>A <see cref="string"/> representing the original string with colors stripped.</returns>
    public static string StripColors(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return text;
        }

        foreach (var number in Numbers)
        {
            text = text.Replace($"\x1b[{number}m", "", StringComparison.Ordinal);
        }

        return text;
    }
}
