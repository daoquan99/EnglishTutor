using EnglishTutor.BuildingBlocks.Application.Abstractions;
using EnglishTutor.BuildingBlocks.Application.Results;
using EnglishTutor.Modules.AI.Application.Abstractions;
using EnglishTutor.Modules.AI.Application.Shared.DTOs;
using EnglishTutor.Modules.AI.Application.Shared.Mappers;

namespace EnglishTutor.Modules.AI.Application.Queries.GetRuntimeRoutes;

public sealed class GetAiRuntimeRoutesQueryHandler(IAiRuntimeRouteRepository routeRepository)
    : IQueryHandler<GetAiRuntimeRoutesQuery, IReadOnlyList<AiRuntimeRouteResponse>>
{
    public async Task<Result<IReadOnlyList<AiRuntimeRouteResponse>>> Handle(GetAiRuntimeRoutesQuery request, CancellationToken cancellationToken)
    {
        var routes = await routeRepository.ListAsync(cancellationToken);
        return routes.Select(route => route.ToResponse()).ToList();
    }
}
