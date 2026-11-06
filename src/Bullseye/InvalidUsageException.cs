namespace Bullseye;

/// <summary>
/// Thrown when invalid arguments are passed when running or listing targets.
/// </summary>
public sealed class InvalidUsageException : Exception
{
    /// <inheritdoc/>
    public InvalidUsageException()
    {
    }

    /// <inheritdoc/>
    public InvalidUsageException(string message) : base(message)
    {
    }

    /// <inheritdoc/>
    public InvalidUsageException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
