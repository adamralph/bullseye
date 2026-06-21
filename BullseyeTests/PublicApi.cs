using System.Text;
using Bullseye;
using PublicApiGenerator;
using Xunit;

namespace BullseyeTests;

public static class PublicApi
{
    [Fact]
    public static async Task IsVerified()
    {
        var options = new ApiGeneratorOptions
        {
            IncludeAssemblyAttributes = false,
            SplitMethodParametersAcrossLines = count => count > 0
        };

        var publicApi = typeof(Targets).Assembly.GeneratePublicApi(options);
        publicApi = RemoveRedundantNamespaceQualifiers(publicApi);
        publicApi = RemoveImplicitUsingQualifiers(publicApi);

        _ = await Verify(publicApi, "cs");
    }

    private static string RemoveRedundantNamespaceQualifiers(string api)
    {
        var builder = new StringBuilder();
        string? currentNamespace = null;
        var depth = 0;

        foreach (var line in api.Split('\n'))
        {
            var trimmed = line.TrimEnd('\r');

            if (trimmed.StartsWith("namespace ", StringComparison.Ordinal))
            {
                currentNamespace = trimmed["namespace ".Length..];
                _ = builder.Append(trimmed).Append('\n');
                continue;
            }

            depth += trimmed.Count(c => c == '{') - trimmed.Count(c => c == '}');
            if (depth == 0) currentNamespace = null;

            _ = currentNamespace != null
                ? builder.Append(trimmed.Replace(currentNamespace + ".", "", StringComparison.Ordinal)).Append('\n')
                : builder.Append(trimmed).Append('\n');
        }

        return builder.ToString();
    }

    private static string RemoveImplicitUsingQualifiers(string api)
    {
        // Sdk="Microsoft.NET.Sdk"
        string[] sdkImplicitNamespacePrefixes =
        [
            "System.",
            "System.Collections.Generic.",
            "System.IO.",
            "System.Linq.",
            "System.Net.Http.",
            "System.Net.Http.Json.",
            "System.Threading.",
            "System.Threading.Tasks."
        ];

        // Replace in reverse alpha order so e.g. System.Threading.Tasks. is replaced before System.
        foreach (var prefix in sdkImplicitNamespacePrefixes.OrderDescending(StringComparer.Ordinal))
        {
            var index = 0;
            while ((index = api.IndexOf(prefix, index, StringComparison.Ordinal)) >= 0)
            {
                var afterPrefix = index + prefix.Length;
                var nextDot = api.IndexOf('.', afterPrefix);
                var nextNonId = afterPrefix;
                while (nextNonId < api.Length && char.IsLetterOrDigit(api[nextNonId])) nextNonId++;

                if (nextDot < 0 || nextDot > nextNonId)
                    api = api.Remove(index, prefix.Length);
                else
                    index = afterPrefix;
            }
        }

        return api;
    }
}
