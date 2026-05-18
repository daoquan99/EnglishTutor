using EnglishTutor.Modules.Users.Contracts.ReadModels;

namespace EnglishTutor.Modules.Users.Contracts.Readers;

public interface IUserLanguageSettingsReader
{
    Task<UserLanguageSettingsReadModel?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken);
}
