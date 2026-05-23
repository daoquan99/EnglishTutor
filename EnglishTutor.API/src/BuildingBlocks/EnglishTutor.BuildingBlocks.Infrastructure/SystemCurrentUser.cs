using EnglishTutor.BuildingBlocks.Application.Abstractions;

namespace EnglishTutor.BuildingBlocks.Infrastructure;

public sealed class SystemCurrentUser : ICurrentUser
{
    public Guid UserId => Guid.Empty;
    public bool IsAuthenticated => false;
    public string? Email => null;
    public bool IsAdmin => false;
    public IReadOnlyCollection<string> Permissions => [];
    public bool HasPermission(string permissionCode) => false;
}
