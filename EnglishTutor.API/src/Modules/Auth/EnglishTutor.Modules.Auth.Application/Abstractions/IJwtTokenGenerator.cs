using EnglishTutor.Modules.Auth.Domain.AuthPermission;
using EnglishTutor.Modules.Auth.Domain.AuthRole;
using EnglishTutor.Modules.Auth.Domain.AuthRole.Entities;
using EnglishTutor.Modules.Auth.Domain.AuthSecurityEvent;
using EnglishTutor.Modules.Auth.Domain.AuthSession;
using EnglishTutor.Modules.Auth.Domain.AuthSession.Entities;
using EnglishTutor.Modules.Auth.Domain.AuthUser;
using EnglishTutor.Modules.Auth.Domain.AuthUser.Entities;

namespace EnglishTutor.Modules.Auth.Application.Abstractions;

public interface IJwtTokenGenerator
{
    GeneratedAccessToken Generate(AuthUser user, IReadOnlyCollection<string> permissionCodes);
}

public sealed record GeneratedAccessToken(string Token, DateTime ExpiresAtUtc);
