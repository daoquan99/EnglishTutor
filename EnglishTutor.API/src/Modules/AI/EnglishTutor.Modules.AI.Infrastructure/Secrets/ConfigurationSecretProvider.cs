using Microsoft.Extensions.Configuration;

namespace EnglishTutor.Modules.AI.Infrastructure.Secrets;

public sealed class ConfigurationSecretProvider(IConfiguration configuration) : ISecretProvider
{
    public string? GetSecret(string secretName)
    {
        if (string.IsNullOrWhiteSpace(secretName))
        {
            return null;
        }

        var normalizedSecretName = secretName.Trim();
        return configuration[normalizedSecretName]
            ?? configuration[normalizedSecretName.Replace("__", ":", StringComparison.Ordinal)]
            ?? configuration[MapLegacyAiSecretName(normalizedSecretName)];
    }

    public string GetRequiredSecret(string secretName)
    {
        var secret = GetSecret(secretName);
        if (string.IsNullOrWhiteSpace(secret))
        {
            throw new InvalidOperationException($"Required secret '{secretName}' is not configured.");
        }

        return secret;
    }

    private static string MapLegacyAiSecretName(string secretName)
    {
        const string prefix = "AI__";
        const string suffix = "__ApiKey";

        if (!secretName.StartsWith(prefix, StringComparison.OrdinalIgnoreCase) ||
            !secretName.EndsWith(suffix, StringComparison.OrdinalIgnoreCase))
        {
            return secretName;
        }

        var providerName = secretName[prefix.Length..^suffix.Length].ToUpperInvariant();
        return $"AI_PROVIDERS:{providerName}:API_KEY";
    }
}
