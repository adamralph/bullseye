namespace Bullseye.Internal
{
    using System;
    using System.IO;

    public class SystemConsole : IConsole
    {
        public TextWriter Error => Console.Error;

        public TextWriter Out => Console.Out;
    }
}
