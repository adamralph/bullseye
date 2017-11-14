namespace BullseyeTests.Infra
{
    using System.IO;

    public static class TextWriterExtensions
    {
        public static string Read(this TextWriter writer)
        {
            writer.Flush();
            return writer.ToString();
        }
    }
}
