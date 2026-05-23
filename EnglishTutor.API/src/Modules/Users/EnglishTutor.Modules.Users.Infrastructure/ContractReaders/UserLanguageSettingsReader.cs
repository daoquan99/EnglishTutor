using EnglishTutor.Modules.Users.Contracts.Readers;
using EnglishTutor.Modules.Users.Contracts.ReadModels;
using EnglishTutor.Modules.Users.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EnglishTutor.Modules.Users.Infrastructure.ContractReaders;

public sealed class UserLanguageSettingsReader(UsersDbContext dbContext) : IUserLanguageSettingsReader
{
    public async Task<UserLanguageSettingsReadModel?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken)
    {
        var settings = await dbContext.UserLanguageSettings
            .SingleOrDefaultAsync(item => item.UserId == userId, cancellationToken);
        if (settings is null)
        {
            return null;
        }

        var activeTarget = await dbContext.UserTargetLanguages
            .SingleAsync(language => language.UserId == userId && language.IsActive, cancellationToken);

        return new UserLanguageSettingsReadModel(
            settings.UserId,
            settings.NativeLanguageCode.Value,
            settings.ActiveTargetLanguageCode.Value,
            settings.UiLanguageCode.Value,
            settings.ExplanationLanguageCode.Value,
            activeTarget.CurrentLevel.ToString(),
            activeTarget.TargetLevel.ToString());
    }
}
