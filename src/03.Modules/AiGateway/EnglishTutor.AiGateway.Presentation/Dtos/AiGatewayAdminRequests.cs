using System;

namespace EnglishTutor.AiGateway.Presentation.Dtos;

public sealed record CreateProviderRequest(string Name, string Code, bool IsActive);
public sealed record UpdateProviderRequest(string Name, bool IsActive);

public sealed record CreateModelRequest(
    Guid ProviderId,
    string DisplayName,
    string Code,
    string ProviderModelId,
    string[]? Capabilities,
    bool? ThinkingEnabled,
    string Lifecycle,
    bool IsActive);

public sealed record UpdateModelRequest(
    string DisplayName,
    string ProviderModelId,
    string[]? Capabilities,
    bool? ThinkingEnabled,
    string Lifecycle,
    bool IsActive);

public sealed record UpdateVoiceRequest(
    string DisplayName,
    string Style,
    string Gender,
    bool IsActive);

public sealed record SetModelVoicesRequest(
    Guid[] VoiceIds,
    Guid DefaultVoiceId);

public sealed record CreateProviderKeyRequest(Guid ProviderId, string Name, string Secret, int Priority, bool IsActive);

public sealed record CreateRoutingRuleRequest(
    string Name, string ActivityType, string TopicCode, string ScenarioCode,
    Guid PrimaryModelId, Guid? FallbackModelId, bool IsActive);

public sealed record UpdateRoutingRuleRequest(
    string Name, string ActivityType, string TopicCode, string ScenarioCode,
    Guid PrimaryModelId, Guid? FallbackModelId, bool IsActive);

public sealed record SetActiveRequest(bool IsActive);
