namespace EnglishLiveTutor.Application.Features.Identity;

public sealed record IdentityPrincipal(Guid Id, string Email, string DisplayName,
    string[] Roles, string[] Permissions);
public sealed record TokenPair(string AccessToken, string RefreshToken,
    DateTimeOffset AccessTokenExpiresAt, DateTimeOffset RefreshTokenExpiresAt);
public sealed record AuthResult(IdentityPrincipal User, TokenPair Tokens);
public sealed record IdentityUserSummary(Guid Id, string Email, string DisplayName, bool IsEnabled, string[] Roles);
public sealed record RoleSummary(Guid Id, string Name, string Description, string[] Permissions);

public interface IIdentityService
{
    Task<IdentityPrincipal?> RegisterAsync(string email, string displayName, string password, CancellationToken ct);
    Task<IdentityPrincipal?> VerifyCredentialsAsync(string email, string password, CancellationToken ct);
    Task<IdentityPrincipal?> FindUserAsync(Guid id, CancellationToken ct);
    Task<(IdentityPrincipal User, string RefreshToken, DateTimeOffset ExpiresAt)?> RotateRefreshTokenAsync(
        string presentedToken, CancellationToken ct);
    Task<(string RefreshToken, DateTimeOffset ExpiresAt)> CreateRefreshTokenAsync(Guid userId, CancellationToken ct);
    Task RevokeRefreshTokenAsync(string presentedToken, Guid? expectedUserId, CancellationToken ct);
    Task<IReadOnlyList<IdentityUserSummary>> ListUsersAsync(CancellationToken ct);
    Task<IReadOnlyList<RoleSummary>> ListRolesAsync(CancellationToken ct);
    Task<IReadOnlyList<string>> ListPermissionsAsync(CancellationToken ct);
    Task<bool> SetUserEnabledAsync(Guid userId, bool enabled, CancellationToken ct);
    Task<bool> SetUserRoleAsync(Guid userId, string role, bool assign, CancellationToken ct);
    Task<bool> CreateRoleAsync(string name, string description, CancellationToken ct);
    Task<bool> DeleteRoleAsync(string name, CancellationToken ct);
    Task<bool> CreatePermissionAsync(string code, string description, CancellationToken ct);
    Task<bool> DeletePermissionAsync(string code, CancellationToken ct);
    Task<bool> SetRolePermissionAsync(string role, string permission, bool grant, CancellationToken ct);
}
