using System;
using System.Collections.Generic;

namespace EnglishTutor.AiGateway.Application.Admin;

// Safe read/write models for AiGateway administration. Provider key secrets are
// only ever accepted as input; responses expose the mask, never the raw or
// encrypted secret.

public sealed record ProviderView(Guid Id, string Name, string Code, bool IsActive);

public sealed record CreateProviderInput(string Name, string Code, bool IsActive);
public sealed record UpdateProviderInput(string Name, bool IsActive);

public sealed record ModelView(
    Guid Id, Guid ProviderId, string Name, string Code, IReadOnlyList<string> Capabilities, bool IsActive);

public sealed record CreateModelInput(Guid ProviderId, string Name, string Code, string[] Capabilities, bool IsActive);
public sealed record UpdateModelInput(string Name, string[] Capabilities, bool IsActive);

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
