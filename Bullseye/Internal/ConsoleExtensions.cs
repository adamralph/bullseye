#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
namespace Bullseye.Internal
{
    using System;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Threading.Tasks;

    public static class ConsoleExtensions
    {
        public static async Task<(Output, Logger)> Initialize(Options options, string logPrefix)
        {
            if (logPrefix == null)
            {
                logPrefix = "Bullseye";
                var entryAssembly = Assembly.GetEntryAssembly();
                if (entryAssembly == null)
                {
                    await Console.Error.WriteLineAsync($"{logPrefix}: Failed to get the entry assembly. Using default log prefix \"{logPrefix}\".").Tax();
                }
                else
                {
                    logPrefix = entryAssembly.GetName().Name;
                }
            }

            if (options.Clear)
            {
                try
                {
                    Console.Clear();
                }
#pragma warning disable CA1031 // Do not catch general exception types
                catch (Exception ex)
#pragma warning restore CA1031 // Do not catch general exception types
                {
                    await Console.Error.WriteLineAsync($"{logPrefix}: Failed to clear the console: {ex}").Tax();
                }
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
                await WindowsConsole.TryEnableVirtualTerminalProcessing(options.Verbose ? Console.Error : NullTextWriter.Instance, logPrefix).Tax();
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
                else if (Environment.GetEnvironmentVariable("GITHUB_ACTIONS")?.ToUpperInvariant() == "TRUE")
                {
                    options.Host = Host.GitHubActions;
                }
                else if (Environment.GetEnvironmentVariable("GITLAB_CI")?.ToUpperInvariant() == "TRUE")
                {
                    options.Host = Host.GitLabCI;
                }
                else if (!string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("TRAVIS_OS_NAME")))
                {
                    options.Host = Host.Travis;
                }
                else if (!string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("TEAMCITY_PROJECT_NAME")))
                {
                    options.Host = Host.TeamCity;
                }
                else if (Environment.GetEnvironmentVariable("TERM_PROGRAM")?.ToUpperInvariant() == "VSCODE")
                {
                    options.Host = Host.VisualStudioCode;
                }
            }

            var palette = new Palette(options.NoColor, options.Host, operatingSystem);
            var output = new Output(Console.Out, palette);
            var log = new Logger(Console.Error, logPrefix, options.SkipDependencies, options.DryRun, options.Parallel, palette, options.Verbose);

            await log.Version().Tax();
            await log.Verbose($"Host: {options.Host}{(options.Host != Host.Unknown ? $" ({(isHostDetected ? "detected" : "forced")})" : "")}").Tax();
            await log.Verbose($"OS: {operatingSystem}").Tax();

            return (output, log);
        }
    }
}
