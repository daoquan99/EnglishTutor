using System.Security.Claims;
using EnglishTutor.BuildingBlocks.Application.Abstractions;
using Microsoft.AspNetCore.Http;

namespace EnglishTutor.BuildingBlocks.Infrastructure;

public sealed class CurrentUser(IHttpContextAccessor httpContextAccessor) : ICurrentUser
{
    private const string PermissionClaimType = "permission";
    private const string FullAccessPermission = "admin.full_access";
    private IReadOnlyCollection<string>? _permissions;

    public Guid UserId
    {
        get
        {
            var value = FindClaimValue("sub", ClaimTypes.NameIdentifier);
            return Guid.TryParse(value, out var id) ? id : Guid.Empty;
        }
    }

    public bool IsAuthenticated =>
        httpContextAccessor.HttpContext?.User.Identity?.IsAuthenticated ?? false;

    public string? Email => FindClaimValue("email", ClaimTypes.Email);

    public bool IsAdmin => HasPermission(FullAccessPermission);

    public IReadOnlyCollection<string> Permissions => _permissions ??=
        httpContextAccessor.HttpContext?.User.FindAll(PermissionClaimType)
            .Select(claim => claim.Value)
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray()
        ?? [];

    public bool HasPermission(string permissionCode)
    {
        if (string.IsNullOrWhiteSpace(permissionCode))
        {
            return false;
        }

        var permissions = Permissions;
        return permissions.Contains(FullAccessPermission, StringComparer.OrdinalIgnoreCase) ||
            permissions.Contains(permissionCode.Trim(), StringComparer.OrdinalIgnoreCase);
    }

    private string? FindClaimValue(params string[] claimTypes)
    {
        var user = httpContextAccessor.HttpContext?.User;

        return claimTypes
            .Select(claimType => user?.FindFirst(claimType)?.Value)
            .FirstOrDefault(value => !string.IsNullOrWhiteSpace(value));
    }
}
