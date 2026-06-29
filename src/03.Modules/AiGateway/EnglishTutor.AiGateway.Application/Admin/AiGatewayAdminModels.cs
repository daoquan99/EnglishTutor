using System;
using System.Collections.Generic;

namespace EnglishTutor.AiGateway.Application.Admin;

public sealed record ProviderView(Guid Id, string Name, string Code, bool IsActive);

public sealed record CreateProviderInput(string Name, string Code, bool IsActive);
public sealed record UpdateProviderInput(string Name, bool IsActive);

public sealed record ModelView(
    Guid Id,
    Guid ProviderId,
    string DisplayName,
    string Code,
    string ProviderModelId,
    IReadOnlyList<string> Capabilities,
    bool? ThinkingEnabled,
    string Lifecycle,
    bool IsActive);

public sealed record CreateModelInput(
    Guid ProviderId,
    string DisplayName,
    string Code,
    string ProviderModelId,
    string[] Capabilities,
    bool? ThinkingEnabled,
    string Lifecycle,
    bool IsActive)
{
    public string Name => DisplayName;

    public CreateModelInput(
        Guid providerId,
        string name,
        string code,
        string[] capabilities,
        bool isActive)
        : this(
            providerId,
            name,
            code,
            code,
            capabilities.Length == 0 ? ["content-generation"] : capabilities,
            null,
            "Stable",
            isActive)
    {
    }
}

public sealed record UpdateModelInput(
    string DisplayName,
    string ProviderModelId,
    string[] Capabilities,
    bool? ThinkingEnabled,
    string Lifecycle,
    bool IsActive);

public sealed record VoiceView(
    Guid Id,
    Guid ProviderId,
    string VoiceId,
    string DisplayName,
    string Style,
    string Gender,
    bool IsActive);

public sealed record ProviderKeyView(
    Guid Id, Guid ProviderId, string Name, string KeyMask, int Priority, bool IsActive, DateTime? CooldownUntilUtc);

public sealed record CreateProviderKeyInput(Guid ProviderId, string Name, string Secret, int Priority, bool IsActive);

public sealed record RoutingRuleView(
    Guid Id, string Name, string ActivityType, string TopicCode, string ScenarioCode,
    Guid PrimaryModelId, Guid? FallbackModelId, bool IsActive);

public sealed record CreateRoutingRuleInput(
    string Name, string ActivityType, string TopicCode, string ScenarioCode,
    Guid PrimaryModelId, Guid? FallbackModelId, bool IsActive);

public sealed record UpdateRoutingRuleInput(
    string Name, string ActivityType, string TopicCode, string ScenarioCode,
    Guid PrimaryModelId, Guid? FallbackModelId, bool IsActive);
