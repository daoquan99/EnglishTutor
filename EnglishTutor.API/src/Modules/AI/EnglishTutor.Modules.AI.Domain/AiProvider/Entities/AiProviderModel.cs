using EnglishTutor.BuildingBlocks.Domain;
using EnglishTutor.BuildingBlocks.Domain.Exceptions;
using EnglishTutor.Modules.AI.Domain.Enums;

namespace EnglishTutor.Modules.AI.Domain.Entities;

public sealed class AiProviderModel : Entity<Guid>
{
    public Guid ProviderId { get; private set; }
    public string ModelCode { get; private set; } = string.Empty;
    public string DisplayName { get; private set; } = string.Empty;
    public AiCapabilityType Capability { get; private set; }
    public bool IsEnabled { get; private set; }
    public bool SupportsStreaming { get; private set; }
    public int MaxInputTokens { get; private set; }
    public int MaxOutputTokens { get; private set; }
    public decimal CostPerInput1KTokens { get; private set; }
    public decimal CostPerOutput1KTokens { get; private set; }
    public int Priority { get; private set; }

    private AiProviderModel() { }

    internal static AiProviderModel Create(
        Guid providerId,
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
        ValidateTokenLimit(maxInputTokens, "Max input tokens");
        ValidateTokenLimit(maxOutputTokens, "Max output tokens");
        ValidateCost(costPerInput1KTokens, "Input token cost");
        ValidateCost(costPerOutput1KTokens, "Output token cost");

        return new AiProviderModel
        {
            Id = Guid.NewGuid(),
            ProviderId = providerId,
            ModelCode = Normalize(modelCode, 150, "Model code").ToLowerInvariant(),
            DisplayName = Normalize(displayName, 200, "Model display name"),
            Capability = capability,
            SupportsStreaming = supportsStreaming,
            MaxInputTokens = maxInputTokens,
            MaxOutputTokens = maxOutputTokens,
            CostPerInput1KTokens = costPerInput1KTokens,
            CostPerOutput1KTokens = costPerOutput1KTokens,
            Priority = Math.Max(0, priority),
            IsEnabled = isEnabled
        };
    }

    internal void UpdateConfiguration(
        string displayName,
        bool supportsStreaming,
        int maxInputTokens,
        int maxOutputTokens,
        decimal costPerInput1KTokens,
        decimal costPerOutput1KTokens,
        int priority,
        bool isEnabled)
    {
        ValidateTokenLimit(maxInputTokens, "Max input tokens");
        ValidateTokenLimit(maxOutputTokens, "Max output tokens");
        ValidateCost(costPerInput1KTokens, "Input token cost");
        ValidateCost(costPerOutput1KTokens, "Output token cost");

        DisplayName = Normalize(displayName, 200, "Model display name");
        SupportsStreaming = supportsStreaming;
        MaxInputTokens = maxInputTokens;
        MaxOutputTokens = maxOutputTokens;
        CostPerInput1KTokens = costPerInput1KTokens;
        CostPerOutput1KTokens = costPerOutput1KTokens;
        Priority = Math.Max(0, priority);
        IsEnabled = isEnabled;
    }

    public void Enable() => IsEnabled = true;

    public void Disable() => IsEnabled = false;

    internal static string Normalize(string value, int maxLength, string fieldName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new DomainException($"{fieldName} is required.");
        }

        var normalized = value.Trim();
        if (normalized.Length > maxLength)
        {
            throw new DomainException($"{fieldName} must not exceed {maxLength} characters.");
        }

        return normalized;
    }

    private static void ValidateTokenLimit(int value, string fieldName)
    {
        if (value <= 0)
        {
            throw new DomainException($"{fieldName} must be positive.");
        }
    }

    private static void ValidateCost(decimal value, string fieldName)
    {
        if (value < 0m)
        {
            throw new DomainException($"{fieldName} cannot be negative.");
        }
    }
}
