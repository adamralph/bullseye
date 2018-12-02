namespace Bullseye.Internal
{
    using System;

    public class BullseyeException : Exception
    {
        public BullseyeException()
        {
        }

        public BullseyeException(string message) : base(message)
        {
        }

        public BullseyeException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
