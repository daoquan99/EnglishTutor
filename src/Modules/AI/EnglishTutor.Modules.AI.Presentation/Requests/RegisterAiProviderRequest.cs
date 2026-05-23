namespace EnglishTutor.Modules.AI.Presentation.Requests;

public sealed record RegisterAiProviderRequest(
    string ProviderName,
    string DisplayName,
    string ProviderType,
    string? BaseUrl,
    string? ApiKeySecretName,
    bool IsEnabled);
