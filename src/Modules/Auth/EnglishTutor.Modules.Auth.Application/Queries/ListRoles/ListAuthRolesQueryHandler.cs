using EnglishTutor.BuildingBlocks.Application.Abstractions;
using EnglishTutor.BuildingBlocks.Application.Results;
using EnglishTutor.Modules.Auth.Application.Abstractions;
using EnglishTutor.Modules.Auth.Application.Shared.DTOs;
using EnglishTutor.Modules.Auth.Application.Shared.Mappers;

namespace EnglishTutor.Modules.Auth.Application.Queries.ListRoles;

public sealed class ListAuthRolesQueryHandler(IAuthRolePermissionRepository repository)
    : IQueryHandler<ListAuthRolesQuery, IReadOnlyList<AuthRoleResponse>>
{
    public async Task<Result<IReadOnlyList<AuthRoleResponse>>> Handle(
        ListAuthRolesQuery request,
        CancellationToken cancellationToken)
    {
        var roles = await repository.ListRolesAsync(request.IncludeDisabled, cancellationToken);
        var permissionsByRoleId = await repository.GetPermissionsByRoleIdsAsync(
            roles.Select(role => role.Id).ToArray(),
            cancellationToken);
        var responses = new List<AuthRoleResponse>(roles.Count);

        foreach (var role in roles)
        {
            var permissions = permissionsByRoleId.GetValueOrDefault(role.Id) ?? [];
            responses.Add(role.ToResponse(permissions));
        }

        return responses;
    }
}
