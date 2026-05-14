namespace Bullseye.Internal;

public sealed class Writer(TextWriter textWriter, WriterLevel level)
{
    public WriterLevel Level { get; } = level;

    public async Task SystemAsync(string message) => await textWriter.WriteLineAsync(message).Tax();

    public async Task ErrorAsync(string message) => await textWriter.WriteLineAsync(message).Tax();

    public async Task InfoAsync(Func<string> message)
    {
        if (Level != WriterLevel.Quiet)
        {
            await textWriter.WriteLineAsync(message()).Tax();
        }
    }

    public async Task VerboseAsync(Func<string> message)
    {
        if (Level == WriterLevel.Verbose)
        {
            await textWriter.WriteLineAsync(message()).Tax();
        }
    }
}

public enum WriterLevel
{
    Normal,
    Quiet,
    Verbose
}
