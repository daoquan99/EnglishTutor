using EnglishTutor.BuildingBlocks.Application.Queries;
using EnglishTutor.BuildingBlocks.Domain.Results;
using EnglishTutor.Identity.Application.Abstractions.Persistence;

namespace EnglishTutor.Identity.Application.Users.Queries.ListPermissions;

public sealed class ListPermissionsHandler
    : IQueryHandler<ListPermissionsQuery, IReadOnlyList<PermissionResult>>
{
    private readonly IUserManagementQueryService _queryService;

    public ListPermissionsHandler(IUserManagementQueryService queryService)
    {
        _queryService = queryService;
    }

    public async Task<Result<IReadOnlyList<PermissionResult>>> Handle(
        ListPermissionsQuery request,
        CancellationToken cancellationToken)
    {
        var permissions = await _queryService.ListPermissionsAsync(cancellationToken);
        return Result.Success(permissions);
    }
}
