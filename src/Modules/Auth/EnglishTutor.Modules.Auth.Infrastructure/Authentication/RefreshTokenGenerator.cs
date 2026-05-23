using System.Security.Cryptography;
using EnglishTutor.BuildingBlocks.Application.Abstractions;
using EnglishTutor.Modules.Auth.Application.Abstractions;
using Microsoft.Extensions.Options;

namespace EnglishTutor.Modules.Auth.Infrastructure.Authentication;

public sealed class RefreshTokenGenerator(
    IOptions<JwtOptions> options,
    IRefreshTokenHasher refreshTokenHasher,
    IDateTimeProvider dateTimeProvider) : IRefreshTokenGenerator
{
    private readonly JwtOptions _options = options.Value;

    public GeneratedRefreshToken Generate()
    {
        var tokenBytes = RandomNumberGenerator.GetBytes(64);
        var token = Convert.ToBase64String(tokenBytes);
        var tokenHash = refreshTokenHasher.Hash(token);
        var expiresAtUtc = dateTimeProvider.UtcNow.AddDays(_options.RefreshTokenExpirationDays);
        return new GeneratedRefreshToken(token, tokenHash, expiresAtUtc);
    }
}
