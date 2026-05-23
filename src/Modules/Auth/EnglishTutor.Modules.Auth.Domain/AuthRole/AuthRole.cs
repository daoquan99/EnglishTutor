using EnglishTutor.BuildingBlocks.Domain;
using EnglishTutor.BuildingBlocks.Domain.Exceptions;

namespace EnglishTutor.Modules.Auth.Domain.AuthRole;

public sealed class AuthRole : AggregateRoot<Guid>
{
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public bool IsSystem { get; private set; }
    public bool IsEnabled { get; private set; }

    private AuthRole() { }

    public static AuthRole Create(string name, string description, bool isSystem, DateTime utcNow) =>
        new()
        {
            Id = Guid.NewGuid(),
            Name = NormalizeName(name),
            Description = NormalizeDescription(description),
            IsSystem = isSystem,
            IsEnabled = true
        };

    public void UpdateDescription(string description, DateTime utcNow)
    {
        Description = NormalizeDescription(description);
    }

    public void Update(string name, string description, bool isEnabled, DateTime utcNow)
    {
        Name = NormalizeName(name);
        Description = NormalizeDescription(description);
        IsEnabled = isEnabled;
    }

    public void Enable(DateTime utcNow)
    {
        IsEnabled = true;
    }

    public void Disable(DateTime utcNow)
    {
        IsEnabled = false;
    }

    private static string NormalizeName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainException("Role name is required.");
        }

        var normalized = name.Trim().ToLowerInvariant();
        if (normalized.Length > 100)
        {
            throw new DomainException("Role name must not exceed 100 characters.");
        }

        if (normalized.Any(character => !char.IsLetterOrDigit(character) && character is not '-' and not '_'))
        {
            throw new DomainException("Role name may contain only letters, digits, hyphen, or underscore.");
        }

        return normalized;
    }

    private static string NormalizeDescription(string description)
    {
        if (string.IsNullOrWhiteSpace(description))
        {
            throw new DomainException("Role description is required.");
        }

        var normalized = description.Trim();
        if (normalized.Length > 300)
        {
            throw new DomainException("Role description must not exceed 300 characters.");
        }

        return normalized;
    }
}
