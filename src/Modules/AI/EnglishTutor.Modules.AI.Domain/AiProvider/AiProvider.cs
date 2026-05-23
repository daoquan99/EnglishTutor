using EnglishTutor.BuildingBlocks.Domain;
using EnglishTutor.BuildingBlocks.Domain.Exceptions;
using EnglishTutor.Modules.AI.Domain.Enums;

namespace EnglishTutor.Modules.AI.Domain.Entities;

public sealed class AiProvider : AggregateRoot<Guid>
{
    private readonly List<AiProviderModel> _models = [];

    public string ProviderName { get; private set; } = string.Empty;
    public string DisplayName { get; private set; } = string.Empty;
    public AiProviderType ProviderType { get; private set; }
    public string? BaseUrl { get; private set; }
    public string? ApiKeySecretName { get; private set; }
    public bool IsEnabled { get; private set; }
    public IReadOnlyCollection<AiProviderModel> Models => _models.AsReadOnly();

    private AiProvider() { }

    public static AiProvider Register(
        string providerName,
        string displayName,
        AiProviderType providerType,
        string? baseUrl,
        string? apiKeySecretName,
        bool isEnabled = true) =>
        new()
        {
            Id = Guid.NewGuid(),
            ProviderName = NormalizeProviderName(providerName),
            DisplayName = AiProviderModel.Normalize(displayName, 200, "Provider display name"),
            ProviderType = providerType,
            BaseUrl = NormalizeOptional(baseUrl, 500, "Base url"),
            ApiKeySecretName = NormalizeOptional(apiKeySecretName, 200, "API key secret name"),
            IsEnabled = isEnabled
        };

    public void UpdateSettings(
        string displayName,
        AiProviderType providerType,
        string? baseUrl,
        string? apiKeySecretName,
        bool isEnabled)
    {
        DisplayName = AiProviderModel.Normalize(displayName, 200, "Provider display name");
        ProviderType = providerType;
        BaseUrl = NormalizeOptional(baseUrl, 500, "Base url");
        ApiKeySecretName = NormalizeOptional(apiKeySecretName, 200, "API key secret name");
        IsEnabled = isEnabled;
    }

    public AiProviderModel AddOrUpdateModel(
        string modelCode,
        string displayName,
        AiCapabilityType capability,
        bool supportsStreaming,
        int maxInputTokens,
        int maxOutputTokens,
        decimal costPerInput1KTokens,
        decimal costPerOutput1KTokens,
        int priority,
        bool isEnabled)
    {
        var normalizedCode = AiProviderModel.Normalize(modelCode, 150, "Model code").ToLowerInvariant();
        var existing = _models.SingleOrDefault(model =>
            model.ModelCode == normalizedCode && model.Capability == capability);

        if (existing is not null)
        {
            existing.UpdateConfiguration(
                displayName,
                supportsStreaming,
                maxInputTokens,
                maxOutputTokens,
                costPerInput1KTokens,
                costPerOutput1KTokens,
                priority,
                isEnabled);
            return existing;
        }

        var model = AiProviderModel.Create(
            Id,
            normalizedCode,
            displayName,
            capability,
            supportsStreaming,
            maxInputTokens,
            maxOutputTokens,
            costPerInput1KTokens,
            costPerOutput1KTokens,
            priority,
            isEnabled);
        _models.Add(model);
        return model;
    }

    public void Enable() => IsEnabled = true;

    public void Disable() => IsEnabled = false;

    public AiProviderModel? FindModel(string modelCode, AiCapabilityType capability)
    {
        var normalizedCode = AiProviderModel.Normalize(modelCode, 150, "Model code").ToLowerInvariant();
        return _models.SingleOrDefault(model => model.ModelCode == normalizedCode && model.Capability == capability);
    }

    internal static string NormalizeProviderName(string providerName)
    {
        var normalized = AiProviderModel.Normalize(providerName, 100, "Provider name").ToLowerInvariant();
        if (normalized.Any(character => !char.IsLetterOrDigit(character) && character is not '-' and not '_'))
        {
            throw new DomainException("Provider name may contain only letters, digits, hyphen, or underscore.");
        }

        return normalized;
    }

    private static string? NormalizeOptional(string? value, int maxLength, string fieldName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var normalized = value.Trim();
        if (normalized.Length > maxLength)
        {
            throw new DomainException($"{fieldName} must not exceed {maxLength} characters.");
        }

        return normalized;
    }
}
