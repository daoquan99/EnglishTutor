using System;
using EnglishTutor.AiGateway.Domain.Aggregates.AiModel.Entities;
using EnglishTutor.BuildingBlocks.Domain.Aggregates;

namespace EnglishTutor.AiGateway.Domain.Aggregates.AiModel;

/// <summary>
/// Represents an AI Model aggregate (e.g. gpt-4o, gemini-1.5-pro).
/// </summary>
public class AiModel : AggregateRoot
{
    private readonly List<AiModelVoice> _modelVoices = [];

    public Guid ProviderId { get; private set; }
    public string DisplayName { get; private set; } = default!;
    public string Code { get; private set; } = default!;
    public string ProviderModelId { get; private set; } = default!;
    public AiModelCapability[] Capabilities { get; private set; } = [];
    public bool? ThinkingEnabled { get; private set; }
    public AiModelLifecycle Lifecycle { get; private set; }
    public bool IsActive { get; private set; }
    public long Version { get; private set; }
    public IReadOnlyCollection<AiModelVoice> ModelVoices => _modelVoices.AsReadOnly();

    private AiModel() { }

    public static AiModel Create(
        Guid id,
        Guid providerId,
        string displayName,
        string code,
        string[] capabilities,
        bool isActive) =>
        Create(
            id,
            providerId,
            displayName,
            code,
            code,
            ParseLegacyCapabilities(capabilities),
            null,
            AiModelLifecycle.Stable,
            isActive);

    public static AiModel Create(
        Guid id,
        Guid providerId,
        string displayName,
        string code,
        string providerModelId,
        IEnumerable<AiModelCapability> capabilities,
        bool? thinkingEnabled,
        AiModelLifecycle lifecycle,
        bool isActive)
    {
        if (providerId == Guid.Empty)
            throw new ArgumentException("Provider ID is required.", nameof(providerId));
        if (string.IsNullOrWhiteSpace(displayName))
            throw new ArgumentException("Model display name cannot be empty.", nameof(displayName));
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("Model code cannot be empty.", nameof(code));
        if (string.IsNullOrWhiteSpace(providerModelId))
            throw new ArgumentException("Provider model ID cannot be empty.", nameof(providerModelId));

        var normalizedCapabilities = NormalizeCapabilities(capabilities);
        EnsureLifecycleCanActivate(lifecycle, isActive);

        return new AiModel
        {
            Id = id,
            ProviderId = providerId,
            DisplayName = displayName.Trim(),
            Code = code.ToLowerInvariant().Trim(),
            ProviderModelId = NormalizeProviderModelId(providerModelId),
            Capabilities = normalizedCapabilities,
            ThinkingEnabled = thinkingEnabled,
            Lifecycle = lifecycle,
            IsActive = isActive,
            Version = 1
        };
    }

    public void Update(
        string displayName,
        string providerModelId,
        IEnumerable<AiModelCapability> capabilities,
        bool? thinkingEnabled,
        AiModelLifecycle lifecycle,
        bool isActive)
    {
        if (string.IsNullOrWhiteSpace(displayName))
            throw new ArgumentException("Model display name cannot be empty.", nameof(displayName));
        if (string.IsNullOrWhiteSpace(providerModelId))
            throw new ArgumentException("Provider model ID cannot be empty.", nameof(providerModelId));

        EnsureLifecycleCanActivate(lifecycle, isActive);
        DisplayName = displayName.Trim();
        ProviderModelId = NormalizeProviderModelId(providerModelId);
        Capabilities = NormalizeCapabilities(capabilities);
        ThinkingEnabled = thinkingEnabled;
        Lifecycle = lifecycle;
        IsActive = isActive;
        Version++;
    }

    public void Activate()
    {
        EnsureLifecycleCanActivate(Lifecycle, true);
        IsActive = true;
        Version++;
    }

    public void Deactivate()
    {
        IsActive = false;
        Version++;
    }

    public void MarkDeleted(Guid? deletedByUserId = null)
    {
        base.MarkDeleted(deletedByUserId, DateTime.UtcNow);
        Version++;
    }

    public bool Supports(AiModelCapability capability) => Capabilities.Contains(capability);

    public void SetVoices(IEnumerable<Guid> voiceIds, Guid defaultVoiceId)
    {
        if (!Supports(AiModelCapability.LiveConversation))
        {
            throw new InvalidOperationException("Only live conversation models can have selectable voices.");
        }

        var ids = voiceIds.Distinct().ToArray();
        if (ids.Length == 0 || !ids.Contains(defaultVoiceId))
        {
            throw new ArgumentException("The default voice must be included in the model voice list.", nameof(defaultVoiceId));
        }

        _modelVoices.Clear();
        foreach (var voiceId in ids)
        {
            _modelVoices.Add(AiModelVoice.Create(Id, voiceId, voiceId == defaultVoiceId));
        }

        Version++;
    }

    private static AiModelCapability[] NormalizeCapabilities(IEnumerable<AiModelCapability> capabilities)
    {
        var values = capabilities?.Distinct().Order().ToArray() ?? [];
        if (values.Length == 0)
        {
            throw new ArgumentException("At least one model capability is required.", nameof(capabilities));
        }

        return values;
    }

    private static AiModelCapability[] ParseLegacyCapabilities(IEnumerable<string> capabilities)
    {
        var parsed = capabilities
            .Select(value => value.Replace("-", string.Empty, StringComparison.Ordinal))
            .Select(value => Enum.TryParse<AiModelCapability>(value, true, out var capability)
                ? capability
                : AiModelCapability.ContentGeneration)
            .Distinct()
            .ToArray();

        return parsed.Length == 0 ? [AiModelCapability.ContentGeneration] : parsed;
    }

    private static string NormalizeProviderModelId(string providerModelId)
    {
        var normalized = providerModelId.Trim();
        return normalized.StartsWith("models/", StringComparison.OrdinalIgnoreCase)
            ? normalized["models/".Length..]
            : normalized;
    }

    private static void EnsureLifecycleCanActivate(AiModelLifecycle lifecycle, bool isActive)
    {
        if (lifecycle == AiModelLifecycle.Deprecated && isActive)
        {
            throw new InvalidOperationException("A deprecated model cannot be active.");
        }
    }
}
