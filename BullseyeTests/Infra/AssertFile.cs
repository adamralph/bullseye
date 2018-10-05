namespace BullseyeTests.Infra
{
    using System;
    using System.IO;

    public static class AssertFile
    {
        public static void Contains(string expected, string actualPath)
        {
            if (File.ReadAllText(actualPath) != expected)
            {
                var expectedPath = Path.Combine(
                    Path.GetDirectoryName(actualPath),
                    Path.GetFileNameWithoutExtension(actualPath) + "-expected" + Path.GetExtension(actualPath));

                File.WriteAllText(expectedPath, expected);

                throw new Exception($"{actualPath} does not contain the contents of {expectedPath}.");
            }
        }
    }
}
