using EnglishTutor.BuildingBlocks.Application.Abstractions;
using EnglishTutor.BuildingBlocks.Application.Results;
using EnglishTutor.Modules.AI.Application.Abstractions;
using EnglishTutor.Modules.AI.Application.Shared.DTOs;
using EnglishTutor.Modules.AI.Application.Shared.Mappers;
using EnglishTutor.Modules.AI.Domain.Enums;

namespace EnglishTutor.Modules.AI.Application.Commands.UpsertProviderModel;

public sealed class UpsertAiProviderModelCommandHandler(
    IAiProviderRepository providerRepository)
    : ICommandHandler<UpsertAiProviderModelCommand, AiProviderResponse>
{
    public async Task<Result<AiProviderResponse>> Handle(UpsertAiProviderModelCommand request, CancellationToken cancellationToken)
    {
        if (!Enum.TryParse<AiCapabilityType>(request.Capability, true, out var capability))
        {
            return Result.Failure<AiProviderResponse>(Error.Validation("AI capability is invalid."));
        }

        var provider = await providerRepository.GetByNameAsync(request.ProviderName, cancellationToken);
        if (provider is null)
        {
            return Result.Failure<AiProviderResponse>(Error.NotFound("AI provider", request.ProviderName));
        }

        provider.AddOrUpdateModel(
            request.ModelCode,
            request.DisplayName,
            capability,
            request.SupportsStreaming,
            request.MaxInputTokens,
            request.MaxOutputTokens,
            request.CostPerInput1KTokens,
            request.CostPerOutput1KTokens,
            request.Priority,
            request.IsEnabled);

        await providerRepository.SaveChangesAsync(cancellationToken);
        return provider.ToResponse();
    }
}
