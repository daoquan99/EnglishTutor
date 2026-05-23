using EnglishTutor.BuildingBlocks.Domain;
using EnglishTutor.BuildingBlocks.Domain.Exceptions;

namespace EnglishTutor.Modules.Auth.Domain.AuthPermission;

public sealed class AuthPermission : AggregateRoot<Guid>
{
    public string Code { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public bool IsEnabled { get; private set; }

    private AuthPermission() { }

    public static AuthPermission Create(string code, string description, DateTime utcNow) =>
        new()
        {
            Id = Guid.NewGuid(),
            Code = NormalizeCode(code),
            Description = NormalizeDescription(description),
            IsEnabled = true
        };

    public void UpdateDescription(string description, DateTime utcNow)
    {
        Description = NormalizeDescription(description);
    }

    public void Enable(DateTime utcNow)
    {
        IsEnabled = true;
    }

    public void Disable(DateTime utcNow)
    {
        IsEnabled = false;
    }

    private static string NormalizeCode(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            throw new DomainException("Permission code is required.");
        }

        var normalized = code.Trim().ToLowerInvariant();
        if (normalized.Length > 150)
        {
            throw new DomainException("Permission code must not exceed 150 characters.");
        }

        if (normalized.Any(character => !char.IsLetterOrDigit(character) && character is not '.' and not '-' and not '_'))
        {
            throw new DomainException("Permission code may contain only letters, digits, dot, hyphen, or underscore.");
        }

        return normalized;
    }

    private static string NormalizeDescription(string description)
    {
        if (string.IsNullOrWhiteSpace(description))
        {
            throw new DomainException("Permission description is required.");
        }

        var normalized = description.Trim();
        if (normalized.Length > 300)
        {
            throw new DomainException("Permission description must not exceed 300 characters.");
        }

        return normalized;
    }
}
