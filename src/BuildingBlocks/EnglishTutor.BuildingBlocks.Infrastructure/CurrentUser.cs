using System.Security.Claims;
using EnglishTutor.BuildingBlocks.Application.Abstractions;
using Microsoft.AspNetCore.Http;

namespace EnglishTutor.BuildingBlocks.Infrastructure;

public sealed class CurrentUser(IHttpContextAccessor httpContextAccessor) : ICurrentUser
{
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

    public bool IsAdmin =>
        httpContextAccessor.HttpContext?.User.IsInRole("Admin") ?? false;

    private string? FindClaimValue(params string[] claimTypes)
    {
        var user = httpContextAccessor.HttpContext?.User;

        return claimTypes
            .Select(claimType => user?.FindFirst(claimType)?.Value)
            .FirstOrDefault(value => !string.IsNullOrWhiteSpace(value));
    }
}
