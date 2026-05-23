using EnglishTutor.Modules.Auth.Application.Abstractions;
using EnglishTutor.Modules.Auth.Domain.AuthPermission;
using EnglishTutor.Modules.Auth.Domain.AuthRole;
using EnglishTutor.Modules.Auth.Domain.AuthRole.Entities;
using EnglishTutor.Modules.Auth.Domain.AuthSecurityEvent;
using EnglishTutor.Modules.Auth.Domain.AuthSession;
using EnglishTutor.Modules.Auth.Domain.AuthSession.Entities;
using EnglishTutor.Modules.Auth.Domain.AuthUser;
using EnglishTutor.Modules.Auth.Domain.AuthUser.Entities;
using EnglishTutor.Modules.Auth.Domain.AuthUser.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace EnglishTutor.Modules.Auth.Infrastructure.Persistence.Repositories;

public sealed class AuthRepository(AuthDbContext dbContext) : IAuthRepository
{
    public Task<bool> EmailExistsAsync(Email email, CancellationToken cancellationToken) =>
        dbContext.AuthUsers.AnyAsync(user => user.Email == email, cancellationToken);

    public Task<AuthUser?> GetByIdAsync(Guid userId, CancellationToken cancellationToken) =>
        dbContext.AuthUsers.SingleOrDefaultAsync(user => user.Id == userId, cancellationToken);

    public Task<AuthUser?> GetByEmailAsync(Email email, CancellationToken cancellationToken) =>
        dbContext.AuthUsers.SingleOrDefaultAsync(user => user.Email == email, cancellationToken);

    public Task<UserCredential?> GetCredentialByUserIdAsync(Guid userId, CancellationToken cancellationToken) =>
        dbContext.UserCredentials.SingleOrDefaultAsync(credential => credential.AuthUserId == userId, cancellationToken);

    public Task AddAsync(AuthUser user, UserCredential credential, CancellationToken cancellationToken)
    {
        dbContext.AuthUsers.Add(user);
        dbContext.UserCredentials.Add(credential);
        return Task.CompletedTask;
    }
}
