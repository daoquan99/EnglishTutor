namespace EnglishTutor.Identity.Application.Abstractions.Auth;

/// <summary>
/// Hashes a raw refresh-token value for persistence. Never store the raw
/// token in the database. Implementation may use SHA-256, HMAC, or any
/// cryptographically secure one-way function.
/// </summary>
public interface IRefreshTokenHasher
{
    /// <summary>Compute the canonical hash of a raw refresh-token value.</summary>
    string Hash(string rawTokenValue);
}
