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

public sealed class RefreshTokenRepository(AuthDbContext dbContext) : IRefreshTokenRepository
{
    public Task<RefreshToken?> GetByHashAsync(string tokenHash, CancellationToken cancellationToken) =>
        dbContext.RefreshTokens.SingleOrDefaultAsync(refreshToken => refreshToken.TokenHash == tokenHash, cancellationToken);

    public Task AddAsync(RefreshToken refreshToken, CancellationToken cancellationToken)
    {
        dbContext.RefreshTokens.Add(refreshToken);
        return Task.CompletedTask;
    }
}
