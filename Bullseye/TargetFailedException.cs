using System;

namespace Bullseye;

/// <summary>
/// Thrown when a target fails.
/// </summary>
public class TargetFailedException : Exception
{
    /// <inheritdoc/>
    public TargetFailedException()
    {
    }

    /// <inheritdoc/>
    public TargetFailedException(string message) : base(message)
    {
    }

    /// <inheritdoc/>
    public TargetFailedException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
