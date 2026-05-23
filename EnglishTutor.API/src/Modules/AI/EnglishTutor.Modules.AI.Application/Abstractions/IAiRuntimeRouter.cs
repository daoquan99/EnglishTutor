using EnglishTutor.Modules.AI.Application.Shared.DTOs;
using EnglishTutor.Modules.AI.Domain.Enums;

namespace EnglishTutor.Modules.AI.Application.Abstractions;

public interface IAiRuntimeRouter
{
    Task<AiRuntimeRouteResolution> ResolveAsync(
        AiTaskType taskType,
        AiCapabilityType capability,
        CancellationToken cancellationToken);
}
