using System;

namespace Bullseye
{
    /// <summary>
    /// Contains extension methods for <see cref="Host"/>.
    /// </summary>
    public static class HostExtensions
    {
        /// <summary>
        /// Detects the host if the current host is <c>null</c>.
        /// </summary>
        /// <param name="host">The current host.</param>
        /// <returns>The current or detected host.</returns>
        public static Host DetectIfNull(this Host? host)
        {
            if (host != null)
            {
                return host.Value;
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
