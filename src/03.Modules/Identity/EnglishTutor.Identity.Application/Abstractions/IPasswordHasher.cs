namespace EnglishTutor.Identity.Application.Abstractions;

/// <summary>
/// Hashes and verifies passwords. Implementation lives in Infrastructure
/// and uses <c>Microsoft.AspNetCore.Identity.PasswordHasher</c>.
/// </summary>
public interface IPasswordHasher
{
    /// <summary>
    /// Hashes a plaintext password for storage. The returned string is
    /// self-contained (algorithm, iterations, salt, hash) so the hash
    /// format can be upgraded later without a schema change.
    /// </summary>
    string HashPassword(string plaintextPassword);

    /// <summary>
    /// Verifies a plaintext password against a stored hash. Returns
    /// <see cref="PasswordVerificationResult.Success"/> on match,
    /// <see cref="PasswordVerificationResult.Failed"/> on mismatch,
    /// <see cref="PasswordVerificationResult.SuccessRehashNeeded"/>
    /// when the hash should be regenerated (e.g., when iterations increased).
    /// </summary>
    PasswordVerificationResult VerifyPassword(string plaintextPassword, string storedHash);
}

/// <summary>Mirrors <c>PasswordVerificationResult</c> in ASP.NET Core Identity.</summary>
public enum PasswordVerificationResult
{
    Failed = 0,
    Success = 1,
    SuccessRehashNeeded = 2
}
