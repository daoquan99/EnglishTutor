using System.Security.Cryptography;
using System.Text;
using EnglishTutor.Identity.Application.Abstractions.Auth;

namespace EnglishTutor.Identity.Infrastructure.Security;

/// <summary>
/// Infrastructure implementation of <see cref="IRefreshTokenHasher"/>.
/// Uses SHA-256 (hex-encoded) to compute a deterministic hash of the raw
/// refresh-token value for persistence. Constant-time comparison is
/// performed at lookup time by the database query layer; the hash itself
/// is deterministic so a unique index is possible.
/// </summary>
internal sealed class RefreshTokenHasher : IRefreshTokenHasher
{
    public string Hash(string rawTokenValue)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(rawTokenValue);
        var bytes = Encoding.UTF8.GetBytes(rawTokenValue);
        var hash = SHA256.HashData(bytes);
        return Convert.ToHexString(hash);
    }
}
