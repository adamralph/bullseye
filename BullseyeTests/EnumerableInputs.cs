using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bullseye.Internal;
using Xunit;
using static BullseyeTests.Infra.Helper;

namespace BullseyeTests
{
    public static class EnumerableInputs
    {
        [Fact]
        public static async Task WithInputs()
        {
            // arrange
            var inputsReceived = new List<int>();

            var targets = new TargetCollection
            {
                CreateTarget("default", new[] { 1, 2 }, input => inputsReceived.Add(input))
            };

            // act
            await targets.RunAsync(new List<string>(), _ => false, default, Console.Out, Console.Error, false);

            // assert
            Assert.Equal(2, inputsReceived.Count);
            Assert.Equal(1, inputsReceived[0]);
            Assert.Equal(2, inputsReceived[1]);
        }

        [Fact]
        public static async Task WithoutInputs()
        {
            // arrange
            var ran = false;

            var targets = new TargetCollection
            {
                CreateTarget("default", Enumerable.Empty<object>(), _ => ran = true),
            };

            // act
            await targets.RunAsync(new List<string>(), _ => false, default, Console.Out, Console.Error, false);

            // assert
            Assert.False(ran);
        }
    }
}
