namespace BullseyeTests.Infra
{
    using System;
    using System.IO;
    using Bullseye.Internal;

    public sealed class TestConsole : IConsole, IDisposable
    {
        private readonly StringWriter @out = new StringWriter();

        public TextWriter Out => this.@out;

        public void Dispose() => this.@out.Dispose();
    }
}
