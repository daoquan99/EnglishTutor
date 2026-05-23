using EnglishTutor.BuildingBlocks.Application.Abstractions;
using EnglishTutor.BuildingBlocks.Application.Results;
using EnglishTutor.Modules.AI.Application.Abstractions;
using EnglishTutor.Modules.AI.Application.Shared.DTOs;
using EnglishTutor.Modules.AI.Application.Shared.Mappers;
using EnglishTutor.Modules.AI.Domain.Entities;
using EnglishTutor.Modules.AI.Domain.Enums;

namespace EnglishTutor.Modules.AI.Application.Commands.RegisterProvider;

public sealed class RegisterAiProviderCommandHandler(
    IAiProviderRepository providerRepository)
    : ICommandHandler<RegisterAiProviderCommand, AiProviderResponse>
{
    public async Task<Result<AiProviderResponse>> Handle(RegisterAiProviderCommand request, CancellationToken cancellationToken)
    {
        if (!Enum.TryParse<AiProviderType>(request.ProviderType, true, out var providerType))
        {
            return Result.Failure<AiProviderResponse>(Error.Validation("AI provider type is invalid."));
        }

        var existing = await providerRepository.GetByNameAsync(request.ProviderName, cancellationToken);
        if (existing is not null)
        {
            return Result.Failure<AiProviderResponse>(Error.Conflict("AI provider already exists."));
        }

        var provider = AiProvider.Register(
            request.ProviderName,
            request.DisplayName,
            providerType,
            request.BaseUrl,
            request.ApiKeySecretName,
            request.IsEnabled);

        await providerRepository.AddAsync(provider, cancellationToken);
        await providerRepository.SaveChangesAsync(cancellationToken);

        return provider.ToResponse();
    }
}
