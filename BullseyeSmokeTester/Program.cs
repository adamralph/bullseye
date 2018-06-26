namespace BullseyeSmokeTester
{
    using System;
    using System.Threading.Tasks;
    using static Bullseye.Targets;

    class Program
    {
        static Task Main(string[] args)
        {
            Add("default", DependsOn("worl:d", "exclai: m"));

            Add("hell\"o", () => Console.WriteLine("Hello"));

            Add("comm/a", DependsOn("hell\"o"), () => Console.WriteLine(", "));

            Add("worl:d", DependsOn("comm/a"), () => Console.WriteLine("World"));

            Add("exclai: m", () => Console.WriteLine("!"));

            return RunAsync(args);
        }
    }
}
