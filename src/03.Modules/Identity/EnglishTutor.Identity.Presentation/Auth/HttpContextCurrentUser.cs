using System.Security.Claims;
using EnglishTutor.BuildingBlocks.Application.CurrentUser;
using Microsoft.AspNetCore.Http;

namespace EnglishTutor.Identity.Presentation.Auth;

/// <summary>
/// Adapts ASP.NET Core's <see cref="ClaimsPrincipal"/> to the
/// BuildingBlocks <see cref="ICurrentUser"/> abstraction. Lets Identity
/// handlers and other modules query the current user from the HTTP context
/// without depending on <c>HttpContext</c> directly.
/// </summary>
public sealed class HttpContextCurrentUser : ICurrentUser
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public HttpContextCurrentUser(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    private ClaimsPrincipal? Principal => _httpContextAccessor.HttpContext?.User;

    public Guid? UserId
    {
        get
        {
            var sub = Principal?.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? Principal?.FindFirstValue("sub");
            return Guid.TryParse(sub, out var id) ? id : null;
        }
    }

    public string? Email => Principal?.FindFirstValue(ClaimTypes.Email)
        ?? Principal?.FindFirstValue("email");

    public bool IsAuthenticated => Principal?.Identity?.IsAuthenticated ?? false;

    public IReadOnlyList<string> Roles =>
        Principal?.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList() ?? [];

    public bool IsInRole(string role) => Principal?.IsInRole(role) ?? false;
}
