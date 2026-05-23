using EnglishTutor.BuildingBlocks.Domain;
using EnglishTutor.BuildingBlocks.Domain.Exceptions;
using EnglishTutor.Modules.AI.Domain.Enums;

namespace EnglishTutor.Modules.AI.Domain.Entities;

public sealed class AiRuntimeRoute : AggregateRoot<Guid>
{
    public AiTaskType TaskType { get; private set; }
    public AiCapabilityType Capability { get; private set; }
    public string PreferredProviderName { get; private set; } = string.Empty;
    public string PreferredModelCode { get; private set; } = string.Empty;
    public string? FallbackProviderName { get; private set; }
    public string? FallbackModelCode { get; private set; }
    public int MaxTokens { get; private set; }
    public decimal Temperature { get; private set; }
    public bool IsActive { get; private set; }

    private AiRuntimeRoute() { }

    public static AiRuntimeRoute Configure(
        AiTaskType taskType,
        AiCapabilityType capability,
        string preferredProviderName,
        string preferredModelCode,
        string? fallbackProviderName,
        string? fallbackModelCode,
        int maxTokens,
        decimal temperature,
        bool isActive = true)
    {
        Validate(maxTokens, temperature, fallbackProviderName, fallbackModelCode);

        return new AiRuntimeRoute
        {
            Id = Guid.NewGuid(),
            TaskType = taskType,
            Capability = capability,
            PreferredProviderName = AiProvider.NormalizeProviderName(preferredProviderName),
            PreferredModelCode = AiProviderModel.Normalize(preferredModelCode, 150, "Preferred model code").ToLowerInvariant(),
            FallbackProviderName = string.IsNullOrWhiteSpace(fallbackProviderName) ? null : AiProvider.NormalizeProviderName(fallbackProviderName),
            FallbackModelCode = string.IsNullOrWhiteSpace(fallbackModelCode) ? null : AiProviderModel.Normalize(fallbackModelCode, 150, "Fallback model code").ToLowerInvariant(),
            MaxTokens = maxTokens,
            Temperature = temperature,
            IsActive = isActive
        };
    }

    public void Update(
        string preferredProviderName,
        string preferredModelCode,
        string? fallbackProviderName,
        string? fallbackModelCode,
        int maxTokens,
        decimal temperature,
        bool isActive)
    {
        Validate(maxTokens, temperature, fallbackProviderName, fallbackModelCode);

        PreferredProviderName = AiProvider.NormalizeProviderName(preferredProviderName);
        PreferredModelCode = AiProviderModel.Normalize(preferredModelCode, 150, "Preferred model code").ToLowerInvariant();
        FallbackProviderName = string.IsNullOrWhiteSpace(fallbackProviderName) ? null : AiProvider.NormalizeProviderName(fallbackProviderName);
        FallbackModelCode = string.IsNullOrWhiteSpace(fallbackModelCode) ? null : AiProviderModel.Normalize(fallbackModelCode, 150, "Fallback model code").ToLowerInvariant();
        MaxTokens = maxTokens;
        Temperature = temperature;
        IsActive = isActive;
    }

    public void Activate() => IsActive = true;

    public void Deactivate() => IsActive = false;

    private static void Validate(int maxTokens, decimal temperature, string? fallbackProviderName, string? fallbackModelCode)
    {
        if (maxTokens <= 0)
        {
            throw new DomainException("Max tokens must be positive.");
        }

        if (temperature is < 0m or > 2m)
        {
            throw new DomainException("Temperature must be between 0 and 2.");
        }

        if (string.IsNullOrWhiteSpace(fallbackProviderName) != string.IsNullOrWhiteSpace(fallbackModelCode))
        {
            throw new DomainException("Fallback provider and fallback model must be configured together.");
        }
    }
}
