using EnglishTutor.Modules.Users.Contracts.Readers;
using EnglishTutor.Modules.Users.Contracts.ReadModels;
using EnglishTutor.Modules.Users.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EnglishTutor.Modules.Users.Infrastructure.ContractReaders;

public sealed class UserTargetLanguageReader(UsersDbContext dbContext) : IUserTargetLanguageReader
{
    public async Task<IReadOnlyList<UserTargetLanguageReadModel>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken) =>
        await dbContext.UserTargetLanguages
            .Where(language => language.UserId == userId)
            .Select(language => new UserTargetLanguageReadModel(
                language.Id,
                language.UserId,
                language.TargetLanguageCode.Value,
                language.CurrentLevel.ToString(),
                language.TargetLevel.ToString(),
                language.IsActive))
            .ToListAsync(cancellationToken);
}
