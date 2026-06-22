using EnglishTutor.Identity.Application.Abstractions;

namespace EnglishTutor.Identity.Infrastructure.Security;

/// <summary>
/// BCrypt-based password hasher. Format: <c>$2a$12$...</c> (BCrypt v2a, 12 rounds).
/// BCrypt is preferred over Microsoft.AspNetCore.Identity.PasswordHasher for
/// cross-platform deployments without Data Protection key ring requirements.
/// </summary>
public sealed class PasswordHasher : IPasswordHasher
{
    private const int WorkFactor = 12;

    public string HashPassword(string plaintextPassword)
    {
        if (string.IsNullOrEmpty(plaintextPassword))
        {
            throw new ArgumentException("Password cannot be empty.", nameof(plaintextPassword));
        }
        return BCrypt.Net.BCrypt.HashPassword(plaintextPassword, WorkFactor);
    }

    public PasswordVerificationResult VerifyPassword(string plaintextPassword, string storedHash)
    {
        if (string.IsNullOrEmpty(storedHash) || string.IsNullOrEmpty(plaintextPassword))
        {
            return PasswordVerificationResult.Failed;
        }
        try
        {
            return BCrypt.Net.BCrypt.Verify(plaintextPassword, storedHash)
                ? PasswordVerificationResult.Success
                : PasswordVerificationResult.Failed;
        }
        catch (BCrypt.Net.SaltParseException)
        {
            // Hash format invalid (e.g., wrong algorithm prefix).
            return PasswordVerificationResult.Failed;
        }
    }
}
