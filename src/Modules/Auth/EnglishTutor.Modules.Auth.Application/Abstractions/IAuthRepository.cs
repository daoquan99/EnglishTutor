using EnglishTutor.Modules.Auth.Domain.Entities;
using EnglishTutor.Modules.Auth.Domain.ValueObjects;

namespace EnglishTutor.Modules.Auth.Application.Abstractions;

public interface IAuthRepository
{
    Task<bool> EmailExistsAsync(Email email, CancellationToken cancellationToken);

    Task<AuthUser?> GetByIdAsync(Guid userId, CancellationToken cancellationToken);

    Task<AuthUser?> GetByEmailAsync(Email email, CancellationToken cancellationToken);

    Task<UserCredential?> GetCredentialByUserIdAsync(Guid userId, CancellationToken cancellationToken);

    Task AddAsync(AuthUser user, UserCredential credential, CancellationToken cancellationToken);
}
