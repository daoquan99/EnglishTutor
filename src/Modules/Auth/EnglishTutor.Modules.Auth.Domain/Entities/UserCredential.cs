using EnglishTutor.BuildingBlocks.Domain;
using EnglishTutor.BuildingBlocks.Domain.Exceptions;
using EnglishTutor.Modules.Auth.Domain.ValueObjects;

namespace EnglishTutor.Modules.Auth.Domain.Entities;

public sealed class UserCredential : Entity<Guid>
{
    public Guid AuthUserId { get; private set; }
    public HashedPassword HashedPassword { get; private set; } = default!;
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime? UpdatedAtUtc { get; private set; }

    private UserCredential() { }

    public static UserCredential Create(Guid authUserId, HashedPassword hashedPassword)
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
            HashedPassword = hashedPassword,
            CreatedAtUtc = DateTime.UtcNow
        };
    }

    public void UpdatePassword(HashedPassword newHashedPassword)
    {
        if (newHashedPassword is null)
        {
            throw new DomainException("Hashed password is required.");
        }

        HashedPassword = newHashedPassword;
        UpdatedAtUtc = DateTime.UtcNow;
    }
}
