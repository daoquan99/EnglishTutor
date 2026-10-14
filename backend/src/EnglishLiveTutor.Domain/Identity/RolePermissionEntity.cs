using EnglishLiveTutor.Domain.Common.Primitives;

namespace EnglishLiveTutor.Domain.Identity;

public sealed class RolePermissionEntity : Entity<Guid>
{
    public Guid RoleId { get; private set; }
    public Guid PermissionId { get; private set; }
    private RolePermissionEntity() { }
    public RolePermissionEntity(Guid roleId, Guid permissionId)
    {
        Id = Guid.NewGuid(); RoleId = roleId; PermissionId = permissionId;
    }
}
