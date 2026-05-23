using EnglishTutor.BuildingBlocks.Application.Abstractions;
using EnglishTutor.Modules.AI.Application.Shared.DTOs;

namespace EnglishTutor.Modules.AI.Application.Queries.GetRuntimeRoutes;

public sealed record GetAiRuntimeRoutesQuery : IQuery<IReadOnlyList<AiRuntimeRouteResponse>>;
