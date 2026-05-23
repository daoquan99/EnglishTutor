using EnglishTutor.Modules.Auth.Application.Shared.DTOs;
using EnglishTutor.Modules.Auth.Domain.AuthPermission;
using EnglishTutor.Modules.Auth.Domain.AuthRole;
using EnglishTutor.Modules.Auth.Domain.AuthRole.Entities;
using EnglishTutor.Modules.Auth.Domain.AuthSecurityEvent;
using EnglishTutor.Modules.Auth.Domain.AuthSession;
using EnglishTutor.Modules.Auth.Domain.AuthSession.Entities;
using EnglishTutor.Modules.Auth.Domain.AuthUser;
using EnglishTutor.Modules.Auth.Domain.AuthUser.Entities;

namespace EnglishTutor.Modules.Auth.Application.Shared.Mappers;

internal static class AuthAdminMappers
{
    public static AuthPermissionResponse ToResponse(this AuthPermission permission) =>
        new(permission.Id, permission.Code, permission.Description, permission.IsEnabled);

    public static AuthRoleResponse ToResponse(this AuthRole role, IReadOnlyList<AuthPermission> permissions) =>
        new(
            role.Id,
            role.Name,
            role.Description,
            role.IsSystem,
            role.IsEnabled,
            permissions
                .OrderBy(permission => permission.Code)
                .Select(permission => permission.ToResponse())
                .ToList());
}
