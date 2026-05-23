using EnglishTutor.BuildingBlocks.Domain;
using EnglishTutor.BuildingBlocks.Domain.Exceptions;
using EnglishTutor.Modules.Auth.Domain.AuthUser.ValueObjects;

namespace EnglishTutor.Modules.Auth.Domain.AuthUser.Entities;

public sealed class UserCredential : Entity<Guid>
{
    public Guid AuthUserId { get; private set; }
    public HashedPassword HashedPassword { get; private set; } = default!;
    private UserCredential() { }

    public static UserCredential Create(Guid authUserId, HashedPassword hashedPassword, DateTime utcNow)
    {
        if (authUserId == Guid.Empty)
        {
            throw new DomainException("Auth user id is required.");
        }

        if (hashedPassword is null)
        {
            throw new DomainException("Hashed password is required.");
        }

        return new UserCredential
        {
            Id = Guid.NewGuid(),
            AuthUserId = authUserId,
            HashedPassword = hashedPassword
        };
    }

    public void UpdatePassword(HashedPassword newHashedPassword, DateTime utcNow)
    {
        if (newHashedPassword is null)
        {
            throw new DomainException("Hashed password is required.");
        }

        HashedPassword = newHashedPassword;
    }
}
