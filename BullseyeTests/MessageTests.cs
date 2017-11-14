namespace BullseyeTests
{
    using Bullseye.Internal;
    using Xbehave;
    using Xunit;

    public class MessageTests
    {
        [Scenario]
        [Example(0.000_001D, "1 ns")]
        [Example(0.001D, "1 \u00B5s")] // µs
        [Example(1D, "1 ms")]
        [Example(1_000D, "1 s")]
        [Example(119_000D, "1 min 59 s")]
        [Example(1_000_000D, "16 min 40 s")]
        [Example(1_000_000_000D, "16,667 min")]
        public void Timings(double elapsed, string expectedSubstring, string message)
        {
            $"When a message is created with elapsed time in milliseconds of {elapsed}"
                .x(() => message = "foo".ToTargetSucceeded(new Options(), elapsed));

            $"Then the message contains \"{expectedSubstring}\""
                .x(() => Assert.Contains(expectedSubstring, message));
        }
    }
}
