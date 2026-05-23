using EnglishTutor.BuildingBlocks.Application.Results;

namespace EnglishTutor.Modules.Auth.Application.Shared.Errors;

public static class AuthErrors
{
    public static readonly Error InvalidCredentials =
        Error.Unauthorized("Invalid email or password.");

    public static readonly Error EmailAlreadyExists =
        Error.Conflict("Email already exists.");

    public static readonly Error RefreshTokenExpired =
        Error.Unauthorized("Refresh token has expired.");

    public static readonly Error RefreshTokenRevoked =
        Error.Unauthorized("Refresh token has been revoked.");

    public static readonly Error RefreshTokenNotFound =
        Error.NotFound("Refresh token was not found.");

    public static readonly Error AuthSessionNotFound =
        Error.NotFound("Auth session was not found.");

    public static readonly Error RefreshTokenSuspicious =
        Error.Unauthorized("Refresh token session is suspicious and has been revoked.");

    public static readonly Error RefreshTokenVerificationUnavailable =
        Error.Unauthorized("Refresh token session cannot be verified. Please login again.");

    public static readonly Error UserInactive =
        Error.Forbidden("User is inactive.");

    public static Error UserNotFound(Guid userId) =>
        Error.NotFound("Auth user", userId);

    public static Error RoleNotFound(Guid roleId) =>
        Error.NotFound("Auth role", roleId);

    public static Error PermissionNotFound(Guid permissionId) =>
        Error.NotFound("Auth permission", permissionId);

    public static readonly Error RoleNameAlreadyExists =
        Error.Conflict("Role name already exists.");

    public static readonly Error PermissionCodeAlreadyExists =
        Error.Conflict("Permission code already exists.");

    public static readonly Error InvalidPermissionSelection =
        Error.Validation("One or more permissions are invalid or disabled.");

    public static readonly Error SystemRoleCannotBeDeleted =
        Error.Conflict("System role cannot be deleted.");

    public static readonly Error SystemRoleCannotBeModified =
        Error.Conflict("System role cannot be modified.");

    public static readonly Error RoleAssignedToUsers =
        Error.Conflict("Role is assigned to users and cannot be deleted.");

    public static readonly Error SystemPermissionCannotBeDeleted =
        Error.Conflict("System permission cannot be deleted.");

    public static readonly Error SystemPermissionCannotBeDisabled =
        Error.Conflict("System permission cannot be disabled.");

    public static readonly Error PermissionAssignedToRoles =
        Error.Conflict("Permission is assigned to roles and cannot be deleted.");
}
