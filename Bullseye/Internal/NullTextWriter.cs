namespace Bullseye.Internal
{
    using System.IO;
    using System.Text;

    public sealed class NullTextWriter : TextWriter
    {
        static NullTextWriter()
        {
        }

        private NullTextWriter()
        {
        }

        public static NullTextWriter Instance { get; } = new NullTextWriter();

        public override Encoding Encoding => Encoding.Default;
    }
}
