using EnglishTutor.Modules.Users.Application.Abstractions;
using EnglishTutor.Modules.Users.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace EnglishTutor.Modules.Users.Infrastructure.Persistence.Repositories;

public sealed class UserTargetLanguageRepository(UsersDbContext dbContext) : IUserTargetLanguageRepository
{
    public async Task<IReadOnlyList<UserTargetLanguage>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken)
    {
        var languages = await dbContext.UserTargetLanguages
            .Where(language => language.UserId == userId)
            .ToListAsync(cancellationToken);

        return languages
            .OrderByDescending(language => language.IsActive)
            .ThenBy(language => language.TargetLanguageCode.Value, StringComparer.Ordinal)
            .ToList();
    }

    public Task<UserTargetLanguage?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        dbContext.UserTargetLanguages.SingleOrDefaultAsync(language => language.Id == id, cancellationToken);

    public Task AddAsync(UserTargetLanguage targetLanguage, CancellationToken cancellationToken)
    {
        dbContext.UserTargetLanguages.Add(targetLanguage);
        return Task.CompletedTask;
    }
}
