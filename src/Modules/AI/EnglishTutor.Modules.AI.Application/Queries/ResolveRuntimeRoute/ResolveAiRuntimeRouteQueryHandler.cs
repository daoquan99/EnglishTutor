using EnglishTutor.BuildingBlocks.Application.Abstractions;
using EnglishTutor.BuildingBlocks.Application.Results;
using EnglishTutor.Modules.AI.Application.Abstractions;
using EnglishTutor.Modules.AI.Application.Shared.DTOs;
using EnglishTutor.Modules.AI.Domain.Enums;

namespace EnglishTutor.Modules.AI.Application.Queries.ResolveRuntimeRoute;

public sealed class ResolveAiRuntimeRouteQueryHandler(IAiRuntimeRouter runtimeRouter)
    : IQueryHandler<ResolveAiRuntimeRouteQuery, AiRuntimeRouteResponse>
{
    public async Task<Result<AiRuntimeRouteResponse>> Handle(ResolveAiRuntimeRouteQuery request, CancellationToken cancellationToken)
    {
        if (!Enum.TryParse<AiTaskType>(request.TaskType, true, out var taskType))
        {
            return Result.Failure<AiRuntimeRouteResponse>(Error.Validation("AI task type is invalid."));
        }

        if (!Enum.TryParse<AiCapabilityType>(request.Capability, true, out var capability))
        {
            return Result.Failure<AiRuntimeRouteResponse>(Error.Validation("AI capability is invalid."));
        }

        var route = await runtimeRouter.ResolveAsync(taskType, capability, cancellationToken);
        return new AiRuntimeRouteResponse(
            Guid.Empty,
            route.TaskType.ToString(),
            route.Capability.ToString(),
            route.PreferredProviderName,
            route.PreferredModelCode,
            route.FallbackProviderName,
            route.FallbackModelCode,
            route.MaxTokens,
            route.Temperature,
            IsActive: true);
    }
}
