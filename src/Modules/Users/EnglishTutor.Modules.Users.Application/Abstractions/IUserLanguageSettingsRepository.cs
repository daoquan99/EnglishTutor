using EnglishTutor.Modules.Users.Domain.Entities;

namespace EnglishTutor.Modules.Users.Application.Abstractions;

public interface IUserLanguageSettingsRepository
{
    Task<UserLanguageSettings?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken);

    Task AddAsync(UserLanguageSettings settings, CancellationToken cancellationToken);
}
