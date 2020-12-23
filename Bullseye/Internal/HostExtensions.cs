using System;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable RS0016 // Add public types and members to the declared API
namespace Bullseye.Internal
{
    public static class HostExtensions
    {
        public static (Host, bool) DetectIfUnknown(this Host host)
        {
            if (host != Host.Unknown)
            {
                return (host, false);
            }

            if (Environment.GetEnvironmentVariable("APPVEYOR")?.ToUpperInvariant() == "TRUE")
            {
                return (Host.Appveyor, true);
            }
            else if (Environment.GetEnvironmentVariable("TF_BUILD")?.ToUpperInvariant() == "TRUE")
            {
                return (Host.AzurePipelines, true);
            }
            else if (Environment.GetEnvironmentVariable("GITHUB_ACTIONS")?.ToUpperInvariant() == "TRUE")
            {
                return (Host.GitHubActions, true);
            }
            else if (Environment.GetEnvironmentVariable("GITLAB_CI")?.ToUpperInvariant() == "TRUE")
            {
                return (Host.GitLabCI, true);
            }
            else if (!string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("TRAVIS_OS_NAME")))
            {
                return (Host.Travis, true);
            }
            else if (!string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("TEAMCITY_PROJECT_NAME")))
            {
                return (Host.TeamCity, true);
            }
            else if (Environment.GetEnvironmentVariable("TERM_PROGRAM")?.ToUpperInvariant() == "VSCODE")
            {
                return (Host.VisualStudioCode, true);
            }

            return (host, false);
        }
    }
}
