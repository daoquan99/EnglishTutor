using EnglishTutor.BuildingBlocks.Domain;
using EnglishTutor.BuildingBlocks.Domain.Exceptions;

namespace EnglishTutor.Modules.Auth.Domain.AuthRole.Entities;

public sealed class AuthRolePermission : Entity<Guid>
{
    public Guid RoleId { get; private set; }
    public Guid PermissionId { get; private set; }

    private AuthRolePermission() { }

    public static AuthRolePermission Create(Guid roleId, Guid permissionId, DateTime utcNow)
    {
        if (roleId == Guid.Empty)
        {
            throw new DomainException("Role id is required.");
        }

        if (permissionId == Guid.Empty)
        {
            throw new DomainException("Permission id is required.");
        }

        return new AuthRolePermission
        {
            Id = Guid.NewGuid(),
            RoleId = roleId,
            PermissionId = permissionId
        };
    }
}
