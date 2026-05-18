using EnglishTutor.BuildingBlocks.Domain;
using EnglishTutor.BuildingBlocks.Domain.Exceptions;
using EnglishTutor.Modules.Auth.Domain.Events;
using EnglishTutor.Modules.Auth.Domain.ValueObjects;

namespace EnglishTutor.Modules.Auth.Domain.Entities;

public sealed class AuthUser : AggregateRoot<Guid>
{
    public Email Email { get; private set; } = default!;
    public string DisplayName { get; private set; } = string.Empty;
    public bool IsActive { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }

    private AuthUser() { }

    public static AuthUser Register(Email email, string displayName)
    {
        if (email is null)
        {
            throw new DomainException("Email is required.");
        }

        if (string.IsNullOrWhiteSpace(displayName))
        {
            throw new DomainException("Display name is required.");
        }

        var normalizedDisplayName = displayName.Trim();
        if (normalizedDisplayName.Length is < 2 or > 100)
        {
            throw new DomainException("Display name must be between 2 and 100 characters.");
        }

        var user = new AuthUser
        {
            Id = Guid.NewGuid(),
            Email = email,
            DisplayName = normalizedDisplayName,
            IsActive = true,
            CreatedAtUtc = DateTime.UtcNow
        };

        user.AddDomainEvent(new UserRegisteredDomainEvent(user.Id, email.Value, normalizedDisplayName));

        return user;
    }

    public void Deactivate() => IsActive = false;
    public void Activate() => IsActive = true;
}
