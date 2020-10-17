#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable RS0016 // Add public types and members to the declared API
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
