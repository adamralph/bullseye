using System;
using System.IO;
using System.Text;
using Xunit;
using Xunit.Sdk;

namespace BullseyeTests.Infra
{
    public static class AssertFile
    {
        public static void Contains(string expectedPath, string actual)
        {
            var expected = File.ReadAllText(expectedPath);

            try
            {
                Assert.Equal(expected, actual);
            }
            catch (EqualException ex)
            {
                var actualPath = Path.Combine(
                    Path.GetDirectoryName(expectedPath),
                    Path.GetFileNameWithoutExtension(expectedPath) + "-actual" + Path.GetExtension(expectedPath));

                File.WriteAllText(actualPath, actual, Encoding.UTF8);

                throw new XunitException(
                    $"{ex.Message}{Environment.NewLine}{Environment.NewLine}Expected file: {expectedPath}{Environment.NewLine}Actual file: {actualPath}");
            }
        }
    }
}
