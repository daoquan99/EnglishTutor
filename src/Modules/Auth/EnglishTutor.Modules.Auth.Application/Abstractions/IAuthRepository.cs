using EnglishTutor.Modules.Auth.Domain.AuthPermission;
using EnglishTutor.Modules.Auth.Domain.AuthRole;
using EnglishTutor.Modules.Auth.Domain.AuthRole.Entities;
using EnglishTutor.Modules.Auth.Domain.AuthSecurityEvent;
using EnglishTutor.Modules.Auth.Domain.AuthSession;
using EnglishTutor.Modules.Auth.Domain.AuthSession.Entities;
using EnglishTutor.Modules.Auth.Domain.AuthUser;
using EnglishTutor.Modules.Auth.Domain.AuthUser.Entities;
using EnglishTutor.Modules.Auth.Domain.AuthUser.ValueObjects;

namespace EnglishTutor.Modules.Auth.Application.Abstractions;

public interface IAuthRepository
{
    Task<bool> EmailExistsAsync(Email email, CancellationToken cancellationToken);

    Task<AuthUser?> GetByIdAsync(Guid userId, CancellationToken cancellationToken);

    Task<AuthUser?> GetByEmailAsync(Email email, CancellationToken cancellationToken);

    Task<UserCredential?> GetCredentialByUserIdAsync(Guid userId, CancellationToken cancellationToken);

    Task AddAsync(AuthUser user, UserCredential credential, CancellationToken cancellationToken);
}
