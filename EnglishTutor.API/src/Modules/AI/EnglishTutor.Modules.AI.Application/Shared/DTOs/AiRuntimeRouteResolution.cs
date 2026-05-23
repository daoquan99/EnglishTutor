using EnglishTutor.Modules.AI.Domain.Enums;

namespace EnglishTutor.Modules.AI.Application.Shared.DTOs;

public sealed record AiRuntimeRouteResolution(
    AiTaskType TaskType,
    AiCapabilityType Capability,
    string PreferredProviderName,
    string PreferredModelCode,
    AiProviderType PreferredProviderType,
    string? PreferredBaseUrl,
    string? PreferredApiKeySecretName,
    string? FallbackProviderName,
    string? FallbackModelCode,
    AiProviderType? FallbackProviderType,
    string? FallbackBaseUrl,
    string? FallbackApiKeySecretName,
    int MaxTokens,
    decimal Temperature,
    AiModelType LegacyModel);
