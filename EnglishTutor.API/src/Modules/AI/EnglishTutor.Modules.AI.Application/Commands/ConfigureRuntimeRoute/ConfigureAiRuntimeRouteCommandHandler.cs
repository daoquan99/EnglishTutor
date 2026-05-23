using EnglishTutor.BuildingBlocks.Application.Abstractions;
using EnglishTutor.BuildingBlocks.Application.Results;
using EnglishTutor.Modules.AI.Application.Abstractions;
using EnglishTutor.Modules.AI.Application.Shared.DTOs;
using EnglishTutor.Modules.AI.Application.Shared.Mappers;
using EnglishTutor.Modules.AI.Domain.Entities;
using EnglishTutor.Modules.AI.Domain.Enums;

namespace EnglishTutor.Modules.AI.Application.Commands.ConfigureRuntimeRoute;

public sealed class ConfigureAiRuntimeRouteCommandHandler(
    IAiProviderRepository providerRepository,
    IAiRuntimeRouteRepository routeRepository)
    : ICommandHandler<ConfigureAiRuntimeRouteCommand, AiRuntimeRouteResponse>
{
    public async Task<Result<AiRuntimeRouteResponse>> Handle(ConfigureAiRuntimeRouteCommand request, CancellationToken cancellationToken)
    {
        if (!Enum.TryParse<AiTaskType>(request.TaskType, true, out var taskType))
        {
            return Result.Failure<AiRuntimeRouteResponse>(Error.Validation("AI task type is invalid."));
        }

        if (!Enum.TryParse<AiCapabilityType>(request.Capability, true, out var capability))
        {
            return Result.Failure<AiRuntimeRouteResponse>(Error.Validation("AI capability is invalid."));
        }

        var validation = await ValidateModelAsync(
            request.PreferredProviderName,
            request.PreferredModelCode,
            capability,
            cancellationToken);
        if (validation is not null)
        {
            return Result.Failure<AiRuntimeRouteResponse>(validation);
        }

        if (string.IsNullOrWhiteSpace(request.FallbackProviderName) !=
            string.IsNullOrWhiteSpace(request.FallbackModelCode))
        {
            return Result.Failure<AiRuntimeRouteResponse>(
                Error.Validation("Fallback provider and fallback model must be configured together."));
        }

        if (!string.IsNullOrWhiteSpace(request.FallbackProviderName) ||
            !string.IsNullOrWhiteSpace(request.FallbackModelCode))
        {
            validation = await ValidateModelAsync(
                request.FallbackProviderName ?? string.Empty,
                request.FallbackModelCode ?? string.Empty,
                capability,
                cancellationToken);
            if (validation is not null)
            {
                return Result.Failure<AiRuntimeRouteResponse>(validation);
            }
        }

        var route = await routeRepository.GetByTaskTypeAsync(taskType, capability, cancellationToken);
        if (route is null)
        {
            route = AiRuntimeRoute.Configure(
                taskType,
                capability,
                request.PreferredProviderName,
                request.PreferredModelCode,
                request.FallbackProviderName,
                request.FallbackModelCode,
                request.MaxTokens,
                request.Temperature,
                request.IsActive);
            await routeRepository.AddAsync(route, cancellationToken);
        }
        else
        {
            route.Update(
                request.PreferredProviderName,
                request.PreferredModelCode,
                request.FallbackProviderName,
                request.FallbackModelCode,
                request.MaxTokens,
                request.Temperature,
                request.IsActive);
        }

        await routeRepository.SaveChangesAsync(cancellationToken);
        return route.ToResponse();
    }

    private async Task<Error?> ValidateModelAsync(
        string providerName,
        string modelCode,
        AiCapabilityType capability,
        CancellationToken cancellationToken)
    {
        var provider = await providerRepository.GetByNameAsync(providerName, cancellationToken);
        if (provider is null)
        {
            return Error.NotFound("AI provider", providerName);
        }

        if (!provider.IsEnabled)
        {
            return Error.Validation($"AI provider '{provider.ProviderName}' is disabled.");
        }

        var model = provider.FindModel(modelCode, capability);
        if (model is null)
        {
            return Error.NotFound("AI provider model", $"{providerName}/{modelCode}/{capability}");
        }

        return model.IsEnabled
            ? null
            : Error.Validation($"AI model '{provider.ProviderName}/{model.ModelCode}' is disabled.");
    }
}
