using EnglishLiveTutor.Domain.Common.Primitives;

namespace EnglishLiveTutor.Domain.Identity;

public sealed class UserRoleEntity : Entity<Guid>
{
    public Guid UserId { get; private set; }
    public Guid RoleId { get; private set; }
    private UserRoleEntity() { }
    public UserRoleEntity(Guid userId, Guid roleId)
    {
        Id = Guid.NewGuid(); UserId = userId; RoleId = roleId;
    }
}
