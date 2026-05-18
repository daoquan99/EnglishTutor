using EnglishTutor.Modules.Users.Application.Abstractions;
using EnglishTutor.Modules.Users.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace EnglishTutor.Modules.Users.Infrastructure.Persistence.Repositories;

public sealed class UserLanguageSettingsRepository(UsersDbContext dbContext) : IUserLanguageSettingsRepository
{
    public Task<UserLanguageSettings?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken) =>
        dbContext.UserLanguageSettings.SingleOrDefaultAsync(settings => settings.UserId == userId, cancellationToken);

    public Task AddAsync(UserLanguageSettings settings, CancellationToken cancellationToken)
    {
        dbContext.UserLanguageSettings.Add(settings);
        return Task.CompletedTask;
    }
}
