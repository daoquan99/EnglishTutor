namespace EnglishTutor.Identity.Application.Abstractions;

/// <summary>
/// Issues and validates JWT access tokens. Implementation lives in
/// Infrastructure and uses <c>System.IdentityModel.Tokens.Jwt</c>.
/// Refresh-token value generation and hashing have moved to dedicated
/// abstractions: <see cref="Auth.IRefreshTokenGenerator"/> and
/// <see cref="Auth.IRefreshTokenHasher"/>.
/// </summary>
public interface IJwtTokenService
{
    /// <summary>
    /// Issues a short-lived access token for the given user.
    /// </summary>
    /// <param name="userId">Subject (sub claim).</param>
    /// <param name="email">Email claim.</param>
    /// <param name="displayName">Display name claim.</param>
    /// <param name="roles">Roles claim (one entry per role).</param>
    /// <param name="permissions">Permissions claim (one entry per permission).</param>
    JwtTokenDescriptor IssueAccessToken(
        Guid userId,
        string email,
        string displayName,
        IReadOnlyCollection<string> roles,
        IReadOnlyCollection<string> permissions);
}

/// <summary>
/// Carries a freshly issued access token plus its absolute expiry so callers
/// can include both in API responses.
/// </summary>
public sealed record JwtTokenDescriptor(string Token, DateTimeOffset ExpiresAt);
