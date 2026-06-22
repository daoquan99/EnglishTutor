using System.Security.Cryptography;
using EnglishTutor.Identity.Application.Abstractions.Auth;

namespace EnglishTutor.Identity.Infrastructure.Security;

/// <summary>
/// Infrastructure implementation of <see cref="IRefreshTokenGenerator"/>.
/// Generates 256 bits of cryptographic randomness, base64url-encoded for
/// safe transport.
/// </summary>
internal sealed class RefreshTokenGenerator : IRefreshTokenGenerator
{
    public string Generate()
    {
        var bytes = new byte[32];
        RandomNumberGenerator.Fill(bytes);
        return Base64UrlEncode(bytes);
    }

    private static string Base64UrlEncode(byte[] bytes) =>
        Convert.ToBase64String(bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_');
}
