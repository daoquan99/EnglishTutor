using EnglishTutor.Identity.Domain.Aggregates.Users.ValueObjects;

namespace EnglishTutor.Identity.Domain.Aggregates.Users.Entities;

/// <summary>
/// Join entity linking users to roles. Composite key (UserId, RoleId).
/// </summary>
public sealed class UserRole
{
    public Guid UserId { get; set; }
    public Guid RoleId { get; set; }
}
