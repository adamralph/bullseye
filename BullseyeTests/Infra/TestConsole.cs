namespace BullseyeTests.Infra
{
    using System;
    using System.IO;
    using Bullseye.Internal;

    public sealed class TestConsole : IConsole, IDisposable
    {
        private readonly StringWriter error = new StringWriter();
        private readonly StringWriter @out = new StringWriter();

        public TextWriter Error => this.error;

        public TextWriter Out => this.@out;

        public void Dispose()
        {
            this.error.Dispose();
            this.@out.Dispose();
        }
    }
}
