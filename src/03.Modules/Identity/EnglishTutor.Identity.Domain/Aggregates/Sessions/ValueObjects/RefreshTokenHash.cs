using System.Security.Cryptography;
using System.Text;

namespace EnglishTutor.Identity.Domain.Aggregates.Sessions.ValueObjects;

/// <summary>
/// Approved Domain value object / hashing API for refresh-token hash format.
/// Wraps a 64-char SHA-256 hex string. The raw refresh-token value is never
/// stored — only its hash. Use <see cref="Compute"/> when persisting a new
/// token; use <see cref="FromHex"/> when rehydrating from storage.
/// </summary>
public sealed class RefreshTokenHash
{
    public string Hex { get; }

    private RefreshTokenHash(string hex)
    {
        Hex = hex;
    }

    /// <summary>
    /// Computes the canonical SHA-256 hash of a raw refresh-token value.
    /// Throws <see cref="ArgumentException"/> if the input is null or whitespace.
    /// </summary>
    public static RefreshTokenHash Compute(string rawTokenValue)
    {
        if (string.IsNullOrWhiteSpace(rawTokenValue))
        {
            throw new ArgumentException("Raw token value is required.", nameof(rawTokenValue));
        }

        var bytes = Encoding.UTF8.GetBytes(rawTokenValue);
        var hash = SHA256.HashData(bytes);
        return new RefreshTokenHash(Convert.ToHexString(hash));
    }

    /// <summary>
    /// Wraps an already-computed hex hash (e.g. from a rehydrated entity).
    /// </summary>
    public static RefreshTokenHash FromHex(string hex)
    {
        if (string.IsNullOrWhiteSpace(hex))
        {
            throw new ArgumentException("Hex value is required.", nameof(hex));
        }
        return new RefreshTokenHash(hex);
    }

    public override string ToString() => Hex;
}
