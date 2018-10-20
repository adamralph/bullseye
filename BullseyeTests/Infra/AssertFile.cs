namespace BullseyeTests.Infra
{
    using System;
    using System.IO;

    public static class AssertFile
    {
        public static void Contains(string expectedPath, string actual)
        {
            if (actual != File.ReadAllText(expectedPath))
            {
                var actualPath = Path.Combine(
                    Path.GetDirectoryName(expectedPath),
                    Path.GetFileNameWithoutExtension(expectedPath) + "-actual" + Path.GetExtension(expectedPath));

                File.WriteAllText(actualPath, actual);

                throw new Exception($"{actualPath} does not contain the contents of {expectedPath}.");
            }
        }
    }
}
