using EnglishTutor.Modules.Users.Domain.Entities;

namespace EnglishTutor.Modules.Users.Application.Abstractions;

public interface IUserTargetLanguageRepository
{
    Task<IReadOnlyList<UserTargetLanguage>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken);

    Task<UserTargetLanguage?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task AddAsync(UserTargetLanguage targetLanguage, CancellationToken cancellationToken);
}
