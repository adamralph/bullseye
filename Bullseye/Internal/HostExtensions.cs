using System;

namespace Bullseye.Internal
{
    public static class HostExtensions
    {
        public static Host DetectIfAutomatic(this Host host)
        {
            if (host != Host.Automatic)
            {
                return host;
            }

            if (Environment.GetEnvironmentVariable("APPVEYOR")?.ToUpperInvariant() == "TRUE")
            {
                return Host.AppVeyor;
            }

            if (Environment.GetEnvironmentVariable("GITHUB_ACTIONS")?.ToUpperInvariant() == "TRUE")
            {
                return Host.GitHubActions;
            }

            if (Environment.GetEnvironmentVariable("GITLAB_CI")?.ToUpperInvariant() == "TRUE")
            {
                return Host.GitLabCI;
            }

            if (!string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("TEAMCITY_PROJECT_NAME")))
            {
                return Host.TeamCity;
            }

            if (!string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("TRAVIS_OS_NAME")))
            {
                return Host.Travis;
            }

#pragma warning disable IDE0046 // Use conditional expression for return
            if (Environment.GetEnvironmentVariable("TERM_PROGRAM")?.ToUpperInvariant() == "VSCODE")
#pragma warning restore IDE0046 // Use conditional expression for return
            {
                return Host.VisualStudioCode;
            }

            return Host.Console;
        }
    }
}
