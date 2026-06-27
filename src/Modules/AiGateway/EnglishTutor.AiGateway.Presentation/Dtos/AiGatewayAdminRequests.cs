using System;

namespace EnglishTutor.AiGateway.Presentation.Dtos;

// Request payloads for AiGateway admin endpoints. Provider key secret is accept-only;
// it is never echoed back. Responses use the Application view models (masked keys).

public sealed record CreateProviderRequest(string Name, string Code, bool IsActive);
public sealed record UpdateProviderRequest(string Name, bool IsActive);

public sealed record CreateModelRequest(Guid ProviderId, string Name, string Code, string[]? Capabilities, bool IsActive);
public sealed record UpdateModelRequest(string Name, string[]? Capabilities, bool IsActive);

public sealed record CreateProviderKeyRequest(Guid ProviderId, string Name, string Secret, int Priority, bool IsActive);

public sealed record CreateRoutingRuleRequest(
    string Name, string ActivityType, string TopicCode, string ScenarioCode,
    Guid PrimaryModelId, Guid? FallbackModelId, bool IsActive);

public sealed record UpdateRoutingRuleRequest(
    string Name, string ActivityType, string TopicCode, string ScenarioCode,
    Guid PrimaryModelId, Guid? FallbackModelId, bool IsActive);

public sealed record SetActiveRequest(bool IsActive);
