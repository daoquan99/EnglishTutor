using EnglishTutor.BuildingBlocks.Domain;
using EnglishTutor.BuildingBlocks.Domain.Exceptions;

namespace EnglishTutor.Modules.Auth.Domain.AuthUser.Entities;

public sealed class AuthUserRole : Entity<Guid>
{
    public Guid AuthUserId { get; private set; }
    public Guid RoleId { get; private set; }

    private AuthUserRole() { }

    public static AuthUserRole Create(Guid authUserId, Guid roleId, DateTime utcNow)
    {
        if (authUserId == Guid.Empty)
        {
            throw new DomainException("Auth user id is required.");
        }

        if (roleId == Guid.Empty)
        {
            throw new DomainException("Role id is required.");
        }

        return new AuthUserRole
        {
            Id = Guid.NewGuid(),
            AuthUserId = authUserId,
            RoleId = roleId
        };
    }
}
