using EnglishTutor.Modules.Auth.Application.Abstractions;
using EnglishTutor.Modules.Auth.Domain.Entities;

namespace EnglishTutor.Modules.Auth.Infrastructure.Persistence.Repositories;

public sealed class AuthSecurityEventRepository(AuthDbContext dbContext) : IAuthSecurityEventRepository
{
    public Task AddAsync(AuthSecurityEvent securityEvent, CancellationToken cancellationToken)
    {
        dbContext.AuthSecurityEvents.Add(securityEvent);
        return Task.CompletedTask;
    }
}
