using EnglishTutor.BuildingBlocks.Application.Abstractions;
using EnglishTutor.Modules.AI.Application.Shared.DTOs;

namespace EnglishTutor.Modules.AI.Application.Queries.ResolveRuntimeRoute;

public sealed record ResolveAiRuntimeRouteQuery(
    string TaskType,
    string Capability) : IQuery<AiRuntimeRouteResponse>;
