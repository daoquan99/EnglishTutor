using EnglishTutor.BuildingBlocks.Application.Abstractions;
using EnglishTutor.BuildingBlocks.Application.Results;
using EnglishTutor.Modules.Auth.Application.Abstractions;
using EnglishTutor.Modules.Auth.Application.Shared.DTOs;
using EnglishTutor.Modules.Auth.Application.Shared.Errors;
using EnglishTutor.Modules.Auth.Application.Shared.Mappers;

namespace EnglishTutor.Modules.Auth.Application.Queries.GetRole;

public sealed class GetAuthRoleQueryHandler(IAuthRolePermissionRepository repository)
    : IQueryHandler<GetAuthRoleQuery, AuthRoleResponse>
{
    public async Task<Result<AuthRoleResponse>> Handle(
        GetAuthRoleQuery request,
        CancellationToken cancellationToken)
    {
        var role = await repository.GetRoleByIdAsync(request.RoleId, cancellationToken);
        if (role is null)
        {
            return Result.Failure<AuthRoleResponse>(AuthErrors.RoleNotFound(request.RoleId));
        }

        var permissions = await repository.GetPermissionsByRoleIdAsync(role.Id, cancellationToken);
        return role.ToResponse(permissions);
    }
}
