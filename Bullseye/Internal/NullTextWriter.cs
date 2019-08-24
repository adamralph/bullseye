#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
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
