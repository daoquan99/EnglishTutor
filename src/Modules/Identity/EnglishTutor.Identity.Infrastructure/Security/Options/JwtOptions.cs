namespace EnglishTutor.Identity.Infrastructure.Security.Options;

/// <summary>
/// Strongly-typed options for JWT token issuance. Bound from configuration
/// section <c>Jwt</c>.
/// </summary>
public sealed class JwtOptions
{
    public const string SectionName = "Jwt";

    /// <summary>Token issuer (iss claim). Must match across API and consumers.</summary>
    public string Issuer { get; set; } = "EnglishTutor.Api";

    /// <summary>Token audience (aud claim). The intended recipient.</summary>
    public string Audience { get; set; } = "EnglishTutor.Clients";

    /// <summary>
    /// HS256 signing key. Production MUST supply a >= 32 byte random secret
    /// via env vars / secret manager (NEVER commit). Dev default fails
    /// startup validation to force explicit override.
    /// </summary>
    public string SigningKey { get; set; } = string.Empty;

    /// <summary>Access-token lifetime in minutes. Default 15.</summary>
    public int AccessTokenMinutes { get; set; } = 15;

    /// <summary>Refresh-token lifetime in days. Default 7.</summary>
    public int RefreshTokenDays { get; set; } = 7;
}
