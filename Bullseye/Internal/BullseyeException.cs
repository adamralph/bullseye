namespace Bullseye.Internal
{
    using System;

#pragma warning disable CA1032 // Implement standard exception constructors
    public class BullseyeException : Exception
#pragma warning restore CA1032 // Implement standard exception constructors
    {
        public BullseyeException(string message) : base(message)
        {
        }
    }
}
