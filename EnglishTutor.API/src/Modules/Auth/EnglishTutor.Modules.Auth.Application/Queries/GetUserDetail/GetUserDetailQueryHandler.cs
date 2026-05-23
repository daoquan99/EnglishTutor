using EnglishTutor.BuildingBlocks.Application.Abstractions;
using EnglishTutor.BuildingBlocks.Application.Results;
using EnglishTutor.Modules.Auth.Application.Abstractions;
using EnglishTutor.Modules.Auth.Application.Shared.DTOs;
using EnglishTutor.Modules.Auth.Application.Shared.Errors;

namespace EnglishTutor.Modules.Auth.Application.Queries.GetUserDetail;

public sealed class GetUserDetailQueryHandler(
    IAuthRepository authRepository,
    IAuthRolePermissionRepository rolePermissionRepository)
    : IQueryHandler<GetUserDetailQuery, AuthUserDetailResponse>
{
    public async Task<Result<AuthUserDetailResponse>> Handle(
        GetUserDetailQuery request,
        CancellationToken cancellationToken)
    {
        var user = await authRepository.GetByIdWithRolesAsync(request.UserId, cancellationToken);
        if (user is null)
        {
            return Result.Failure<AuthUserDetailResponse>(AuthErrors.UserNotFound(request.UserId));
        }

        var assignedRoleIds = user.Roles.Select(role => role.RoleId).ToArray();
        var roles = await rolePermissionRepository.ListRolesAsync(includeDisabled: true, cancellationToken);
        var roleSummaries = roles
            .Where(role => assignedRoleIds.Contains(role.Id))
            .Select(role => new AuthUserRoleSummary(role.Id, role.Name))
            .ToList();

        var permissionsByRoleId = await rolePermissionRepository.GetPermissionsByRoleIdsAsync(
            assignedRoleIds,
            cancellationToken);

        var effectivePermissionCodes = permissionsByRoleId.Values
            .SelectMany(list => list)
            .Where(permission => permission.IsEnabled)
            .Select(permission => permission.Code)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(code => code, StringComparer.OrdinalIgnoreCase)
            .ToList();

        return new AuthUserDetailResponse(
            user.Id,
            user.Email.Value,
            user.DisplayName,
            user.IsActive,
            roleSummaries,
            effectivePermissionCodes,
            user.CreatedAtUtc,
            user.UpdatedAtUtc);
    }
}
