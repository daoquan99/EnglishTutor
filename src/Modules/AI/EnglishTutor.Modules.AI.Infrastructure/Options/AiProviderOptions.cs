using Microsoft.Extensions.Configuration;

namespace EnglishTutor.Modules.AI.Infrastructure.Options;

public sealed class AiProviderOptions
{
    public const string SectionName = "AiProviders";

    public AiProviderOptions(IReadOnlyDictionary<string, AiProviderSetting> providers)
    {
        Providers = providers;
    }

    public IReadOnlyDictionary<string, AiProviderSetting> Providers { get; }

    public static AiProviderOptions FromConfiguration(IConfiguration configuration)
    {
        var providers = configuration
            .GetSection(SectionName)
            .GetChildren()
            .Select(section => new
            {
                ProviderName = NormalizeProviderName(section.Key),
                Setting = section.Get<AiProviderSetting>() ?? new AiProviderSetting()
            })
            .Where(item => !string.IsNullOrWhiteSpace(item.ProviderName))
            .ToDictionary(item => item.ProviderName, item => item.Setting, StringComparer.OrdinalIgnoreCase);

        return new AiProviderOptions(providers);
    }

    public AiProviderSetting? FindProvider(string? providerName)
    {
        if (string.IsNullOrWhiteSpace(providerName))
        {
            return null;
        }

        Providers.TryGetValue(NormalizeProviderName(providerName), out var setting);
        return setting;
    }

    private static string NormalizeProviderName(string providerName) =>
        providerName.Trim().ToLowerInvariant();
}

public sealed class AiProviderSetting
{
    public string? BaseUrl { get; init; }
    public string? ApiKeySecretName { get; init; }
    public string? DefaultModelCode { get; init; }
    public Dictionary<string, string> ModelAliases { get; init; } = new(StringComparer.OrdinalIgnoreCase);
}
