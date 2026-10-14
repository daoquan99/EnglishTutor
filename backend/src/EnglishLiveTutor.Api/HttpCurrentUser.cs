using System.Security.Claims;
using EnglishLiveTutor.Application.Common.Interfaces;

namespace EnglishLiveTutor.Api;

/// <summary>Returns the authenticated actor, or a system identity for non-HTTP saves.</summary>
public sealed class HttpCurrentUser(IHttpContextAccessor contextAccessor) : ICurrentUser
{
    public string? UserId => contextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier)
                            ?? contextAccessor.HttpContext?.User.FindFirstValue("sub")
                            ?? "system";
}
