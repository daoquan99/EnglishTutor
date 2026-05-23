using EnglishTutor.Modules.Users.Contracts.ReadModels;

namespace EnglishTutor.Modules.Users.Contracts.Readers;

public interface IUserTargetLanguageReader
{
    Task<IReadOnlyList<UserTargetLanguageReadModel>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken);
}
