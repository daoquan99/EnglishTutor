using EnglishTutor.Modules.Auth.Application.Abstractions;
using EnglishTutor.Modules.Auth.Domain.Entities;
using EnglishTutor.Modules.Auth.Domain.ValueObjects;
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
