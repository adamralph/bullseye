namespace Bullseye.Internal
{
    using System.IO;

    public interface IConsole
    {
        TextWriter Out { get; }
    }
}
