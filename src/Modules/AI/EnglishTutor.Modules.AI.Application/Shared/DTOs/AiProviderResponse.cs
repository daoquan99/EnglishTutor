namespace EnglishTutor.Modules.AI.Application.Shared.DTOs;

public sealed record AiProviderResponse(
    Guid Id,
    string ProviderName,
    string DisplayName,
    string ProviderType,
    string? BaseUrl,
    string? ApiKeySecretName,
    bool IsEnabled,
    IReadOnlyList<AiProviderModelResponse> Models);
