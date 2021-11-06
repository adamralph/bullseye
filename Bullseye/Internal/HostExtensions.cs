using System;

namespace Bullseye.Internal
{
    public static class HostExtensions
    {
        public static Host DetectIfNull(this Host? host)
        {
            if (host.HasValue)
            {
                return host.Value;
            }

            if (Environment.GetEnvironmentVariable("APPVEYOR")?.ToUpperInvariant() == "TRUE")
            {
                return Host.AppVeyor;
            }
            else if (Environment.GetEnvironmentVariable("TF_BUILD")?.ToUpperInvariant() == "TRUE")
            {
                return Host.AzurePipelines;
            }
            else if (Environment.GetEnvironmentVariable("GITHUB_ACTIONS")?.ToUpperInvariant() == "TRUE")
            {
                return Host.GitHubActions;
            }
            else if (Environment.GetEnvironmentVariable("GITLAB_CI")?.ToUpperInvariant() == "TRUE")
            {
                return Host.GitLabCI;
            }
            else if (!string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("TRAVIS_OS_NAME")))
            {
                return Host.Travis;
            }
            else if (!string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("TEAMCITY_PROJECT_NAME")))
            {
                return Host.TeamCity;
            }
            else if (Environment.GetEnvironmentVariable("TERM_PROGRAM")?.ToUpperInvariant() == "VSCODE")
            {
                return Host.VisualStudioCode;
            }

            return Host.Console;
        }
    }
}
