using EnglishTutor.Modules.Auth.Application.Abstractions;
using EnglishTutor.Modules.Auth.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace EnglishTutor.Modules.Auth.Infrastructure.Persistence.Repositories;

public sealed class AuthSessionRepository(AuthDbContext dbContext) : IAuthSessionRepository
{
    public Task<AuthSession?> GetByIdAsync(Guid sessionId, CancellationToken cancellationToken) =>
        dbContext.AuthSessions.SingleOrDefaultAsync(session => session.Id == sessionId, cancellationToken);

    public Task AddAsync(AuthSession session, CancellationToken cancellationToken)
    {
        dbContext.AuthSessions.Add(session);
        return Task.CompletedTask;
    }
}
