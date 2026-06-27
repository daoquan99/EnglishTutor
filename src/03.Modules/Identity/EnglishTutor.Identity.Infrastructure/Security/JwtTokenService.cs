using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using EnglishTutor.Identity.Application.Abstractions;
using EnglishTutor.Identity.Infrastructure.Security.Options;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace EnglishTutor.Identity.Infrastructure.Security;

/// <summary>
/// Issues HS256-signed JWT access tokens. Refresh-token value generation
/// and hashing live in <see cref="RefreshTokenGenerator"/> and
/// <see cref="RefreshTokenHasher"/> respectively (see
/// <see cref="IRefreshTokenGenerator"/> / <see cref="IRefreshTokenHasher"/>).
/// </summary>
public sealed class JwtTokenService : IJwtTokenService
{
    private readonly JwtOptions _options;
    private readonly SigningCredentials _signingCredentials;

    public JwtTokenService(IOptions<JwtOptions> options)
    {
        _options = options.Value;
        if (string.IsNullOrWhiteSpace(_options.SigningKey) || _options.SigningKey.Length < 32)
        {
            throw new InvalidOperationException(
                "Jwt:SigningKey must be configured and at least 32 characters long. " +
                "Set via appsettings, user-secrets, or environment variable.");
        }

        var keyBytes = Encoding.UTF8.GetBytes(_options.SigningKey);
        _signingCredentials = new SigningCredentials(
            new SymmetricSecurityKey(keyBytes),
            SecurityAlgorithms.HmacSha256);
    }

    public JwtTokenDescriptor IssueAccessToken(
        Guid userId,
        string email,
        string displayName,
        IReadOnlyCollection<string> roles,
        IReadOnlyCollection<string> permissions)
    {
        var now = DateTime.UtcNow;
        var expiresAt = now.AddMinutes(_options.AccessTokenMinutes);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new(JwtRegisteredClaimNames.Email, email),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString("N")),
            new("display_name", displayName ?? string.Empty),
            new(JwtRegisteredClaimNames.Iat, new DateTimeOffset(now).ToUnixTimeSeconds().ToString(),
                ClaimValueTypes.Integer64)
        };
        foreach (var role in roles ?? Array.Empty<string>())
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }
        foreach (var permission in permissions ?? Array.Empty<string>())
        {
            claims.Add(new Claim("permission", permission));
        }

        var token = new JwtSecurityToken(
            issuer: _options.Issuer,
            audience: _options.Audience,
            claims: claims,
            notBefore: now,
            expires: expiresAt,
            signingCredentials: _signingCredentials);

        var encoded = new JwtSecurityTokenHandler().WriteToken(token);
        return new JwtTokenDescriptor(encoded, expiresAt);
    }
}
