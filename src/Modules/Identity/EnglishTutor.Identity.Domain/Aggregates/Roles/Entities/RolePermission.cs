namespace EnglishTutor.Identity.Domain.Aggregates.Roles.Entities;

/// <summary>
/// Join entity linking roles to permissions. Composite key (RoleId, PermissionId).
/// </summary>
public sealed class RolePermission
{
    public Guid RoleId { get; set; }
    public Guid PermissionId { get; set; }
}
