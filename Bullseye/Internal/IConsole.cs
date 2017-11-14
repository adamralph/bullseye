namespace Bullseye.Internal
{
    using System.IO;

    public interface IConsole
    {
        TextWriter Error { get; }

        TextWriter Out { get; }
    }
}
