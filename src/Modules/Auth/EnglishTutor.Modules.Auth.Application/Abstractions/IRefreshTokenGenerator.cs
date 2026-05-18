namespace EnglishTutor.Modules.Auth.Application.Abstractions;

public interface IRefreshTokenGenerator
{
    GeneratedRefreshToken Generate();
}

public sealed record GeneratedRefreshToken(
    string Token,
    string TokenHash,
    DateTime ExpiresAtUtc);
