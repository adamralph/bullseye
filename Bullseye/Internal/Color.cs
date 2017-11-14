namespace Bullseye.Internal
{
    public static class Color
    {
        public static string Default(bool noColor) => noColor ? "" : "\x1b[0m";

        public static string Green(bool noColor) => noColor ? "" : "\x1b[32m";

        public static string Magenta(bool noColor) => noColor ? "" : "\x1b[35m";

        public static string Cyan(bool noColor) => noColor ? "" : "\x1b[36m";

        public static string White(bool noColor) => noColor ? "" : "\x1b[37m";

        public static string BrightRed(bool noColor) => noColor ? "" : "\x1b[91m";

        public static string BrightYellow(bool noColor) => noColor ? "" : "\x1b[93m";

        public static string BrightMagenta(bool noColor) => noColor ? "" : "\x1b[95m";
    }
}
