#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
namespace Bullseye.Internal
{
    using System;

#pragma warning disable CA1032 // Implement standard exception constructors
    public class TargetFailedException : Exception
#pragma warning restore CA1032 // Implement standard exception constructors
    {
        public TargetFailedException(Exception innerException) : base(default, innerException)
        {
        }
    }
}
