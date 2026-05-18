using EnglishTutor.Modules.Auth.Domain.Entities;

namespace EnglishTutor.Modules.Auth.Application.Abstractions;

public interface IJwtTokenGenerator
{
    GeneratedAccessToken Generate(AuthUser user);
}

public sealed record GeneratedAccessToken(string Token, DateTime ExpiresAtUtc);
