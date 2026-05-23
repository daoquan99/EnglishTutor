using EnglishTutor.BuildingBlocks.Domain;
using EnglishTutor.BuildingBlocks.Domain.Exceptions;

namespace EnglishTutor.Modules.Users.Domain.Entities;

public sealed class UserPreference : Entity<Guid>
{
    public Guid UserId { get; private set; }
    public string Key { get; private set; } = string.Empty;
    public string Value { get; private set; } = string.Empty;
    private UserPreference() { }

    public static UserPreference Create(Guid userId, string key, string value, DateTime utcNow)
    {
        if (userId == Guid.Empty)
        {
            throw new DomainException("User id is required.");
        }

        return new UserPreference
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Key = Normalize(key, 100, "Preference key"),
            Value = Normalize(value, 1000, "Preference value"),
            UpdatedAtUtc = utcNow
        };
    }

    public void UpdateValue(string value, DateTime utcNow)
    {
        Value = Normalize(value, 1000, "Preference value");
        UpdatedAtUtc = utcNow;
    }

    private static string Normalize(string value, int maxLength, string fieldName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new DomainException($"{fieldName} is required.");
        }

        var normalized = value.Trim();
        if (normalized.Length > maxLength)
        {
            throw new DomainException($"{fieldName} must not exceed {maxLength} characters.");
        }

        return normalized;
    }
}
