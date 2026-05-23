using EnglishTutor.BuildingBlocks.Domain;
using EnglishTutor.BuildingBlocks.Domain.Exceptions;
using EnglishTutor.Modules.Users.Domain.Events;
using EnglishTutor.Modules.Users.Domain.ValueObjects;

namespace EnglishTutor.Modules.Users.Domain.Entities;

public sealed class UserProfile : AggregateRoot<Guid>
{
    public Guid UserId { get; private set; }
    public DisplayName DisplayName { get; private set; } = default!;
    public string? AvatarUrl { get; private set; }
    public string? Bio { get; private set; }
    private UserProfile() { }

    public static UserProfile Create(Guid userId, string displayName, DateTime utcNow)
    {
        if (userId == Guid.Empty)
        {
            throw new DomainException("User id is required.");
        }

        var profile = new UserProfile
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            DisplayName = DisplayName.Create(displayName),
            CreatedAtUtc = utcNow,
            UpdatedAtUtc = utcNow
        };

        profile.AddDomainEvent(new UserProfileCreatedDomainEvent(userId, profile.DisplayName.Value));
        return profile;
    }

    public void UpdateProfile(string displayName, string? avatarUrl, string? bio, DateTime utcNow)
    {
        DisplayName = DisplayName.Create(displayName);
        AvatarUrl = NormalizeOptionalText(avatarUrl, 2048, "Avatar url");
        Bio = NormalizeOptionalText(bio, 500, "Bio");
        UpdatedAtUtc = utcNow;

        AddDomainEvent(new UserProfileUpdatedDomainEvent(UserId, DisplayName.Value, AvatarUrl, Bio));
    }

    private static string? NormalizeOptionalText(string? value, int maxLength, string fieldName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var normalized = value.Trim();
        if (normalized.Length > maxLength)
        {
            throw new DomainException($"{fieldName} must not exceed {maxLength} characters.");
        }

        return normalized;
    }
}
