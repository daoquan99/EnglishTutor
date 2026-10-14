using EnglishLiveTutor.Domain.Common.Primitives;

namespace EnglishLiveTutor.Domain.Identity;

public sealed class UserEntity : AggregateRoot<Guid>
{
    public string Email { get; private set; } = "";
    public string NormalizedEmail { get; private set; } = "";
    public string DisplayName { get; private set; } = "";
    public string PasswordHash { get; private set; } = "";
    public bool IsEnabled { get; private set; } = true;

    private UserEntity() { }

    public UserEntity(string email, string displayName, string passwordHash)
    {
        Id = Guid.NewGuid();
        Email = email.Trim();
        NormalizedEmail = NormalizeEmail(email);
        DisplayName = displayName.Trim();
        PasswordHash = passwordHash;
    }

    public static string NormalizeEmail(string email) => email.Trim().ToUpperInvariant();
    public void SetPasswordHash(string hash) => PasswordHash = hash;
    public void SetEnabled(bool enabled) => IsEnabled = enabled;
    public void Rename(string displayName) => DisplayName = displayName.Trim();
}
