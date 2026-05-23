using EnglishTutor.Modules.Auth.Application.Abstractions;
using EnglishTutor.Modules.Auth.Domain.AuthPermission;
using EnglishTutor.Modules.Auth.Domain.AuthRole;
using EnglishTutor.Modules.Auth.Domain.AuthRole.Entities;
using EnglishTutor.Modules.Auth.Domain.AuthSecurityEvent;
using EnglishTutor.Modules.Auth.Domain.AuthSession;
using EnglishTutor.Modules.Auth.Domain.AuthSession.Entities;
using EnglishTutor.Modules.Auth.Domain.AuthUser;
using EnglishTutor.Modules.Auth.Domain.AuthUser.Entities;
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
