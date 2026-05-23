using EnglishTutor.Modules.AI.Domain.Enums;

namespace EnglishTutor.Modules.AI.Application.Shared.DTOs;

public sealed record AiRequest(
    AiTaskType TaskType,
    AiModelType Model,
    string Prompt,
    int MaxTokens,
    decimal Temperature,
    string? ProviderName = null,
    string? ModelCode = null,
    AiCapabilityType? Capability = null,
    AiProviderType? ProviderType = null,
    string? BaseUrl = null,
    string? ApiKeySecretName = null);
