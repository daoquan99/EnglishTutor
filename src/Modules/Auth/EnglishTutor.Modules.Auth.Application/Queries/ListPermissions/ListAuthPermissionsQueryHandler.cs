using EnglishTutor.BuildingBlocks.Application.Abstractions;
using EnglishTutor.BuildingBlocks.Application.Results;
using EnglishTutor.Modules.Auth.Application.Abstractions;
using EnglishTutor.Modules.Auth.Application.Shared.DTOs;
using EnglishTutor.Modules.Auth.Application.Shared.Mappers;

namespace EnglishTutor.Modules.Auth.Application.Queries.ListPermissions;

public sealed class ListAuthPermissionsQueryHandler(IAuthRolePermissionRepository repository)
    : IQueryHandler<ListAuthPermissionsQuery, IReadOnlyList<AuthPermissionResponse>>
{
    public async Task<Result<IReadOnlyList<AuthPermissionResponse>>> Handle(
        ListAuthPermissionsQuery request,
        CancellationToken cancellationToken)
    {
        var permissions = await repository.ListPermissionsAsync(request.IncludeDisabled, cancellationToken);
        return permissions.Select(permission => permission.ToResponse()).ToList();
    }
}
