using EnglishTutor.BuildingBlocks.Application.Abstractions;
using EnglishTutor.Modules.AI.Application.Shared.DTOs;

namespace EnglishTutor.Modules.AI.Application.Commands.RegisterProvider;

public sealed record RegisterAiProviderCommand(
    string ProviderName,
    string DisplayName,
    string ProviderType,
    string? BaseUrl,
    string? ApiKeySecretName,
    bool IsEnabled) : ICommand<AiProviderResponse>;
