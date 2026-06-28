using EnglishTutor.BuildingBlocks.Application.Queries;
using EnglishTutor.BuildingBlocks.Domain.Results;
using EnglishTutor.Identity.Application.Abstractions.Persistence;

namespace EnglishTutor.Identity.Application.Users.Queries.ListRoles;

public sealed class ListRolesHandler : IQueryHandler<ListRolesQuery, IReadOnlyList<RoleResult>>
{
    private readonly IUserManagementQueryService _queryService;

    public ListRolesHandler(IUserManagementQueryService queryService)
    {
        _queryService = queryService;
    }

    public async Task<Result<IReadOnlyList<RoleResult>>> Handle(
        ListRolesQuery request,
        CancellationToken cancellationToken)
    {
        var roles = await _queryService.ListRolesAsync(cancellationToken);
        return Result.Success(roles);
    }
}
