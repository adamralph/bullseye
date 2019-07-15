namespace Bullseye.Internal
{
    using System;
    using System.Runtime.InteropServices;
    using System.Threading.Tasks;

    public static class ConsoleExtensions
    {
        public static async Task<Logger> Initialize(Options options)
        {
            if (options.Clear)
            {
                Console.Clear();
            }

            var operatingSystem = OperatingSystem.Unknown;

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                operatingSystem = OperatingSystem.Windows;
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                operatingSystem = OperatingSystem.Linux;
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                operatingSystem = OperatingSystem.MacOS;
            }

            if (!options.NoColor && operatingSystem == OperatingSystem.Windows)
            {
                await WindowsConsole.TryEnableVirtualTerminalProcessing(Console.Out, options.Verbose).Tax();
            }

            var isHostDetected = false;
            if (options.Host == Host.Unknown)
            {
                isHostDetected = true;

                if (Environment.GetEnvironmentVariable("APPVEYOR")?.ToUpperInvariant() == "TRUE")
                {
                    options.Host = Host.Appveyor;
                }
                else if (Environment.GetEnvironmentVariable("TF_BUILD")?.ToUpperInvariant() == "TRUE")
                {
                    options.Host = Host.AzurePipelines;
                }
                else if (!string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("TRAVIS_OS_NAME")))
                {
                    options.Host = Host.Travis;
                }
                else if (!string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("TEAMCITY_PROJECT_NAME")))
                {
                    options.Host = Host.TeamCity;
                }
            }

            var palette = new Palette(options.NoColor, options.Host, operatingSystem);
            var log = new Logger(Console.Out, options.SkipDependencies, options.DryRun, options.Parallel, palette, options.Verbose);

            await log.Version().Tax();
            await log.Verbose($"Host: {options.Host}{(options.Host != Host.Unknown ? $" ({(isHostDetected ? "detected" : "forced")})" : "")}").Tax();
            await log.Verbose($"OS: {operatingSystem}").Tax();

            return log;
        }
    }
}
