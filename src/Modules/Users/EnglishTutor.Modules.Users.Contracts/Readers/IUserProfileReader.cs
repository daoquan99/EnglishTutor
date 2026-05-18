using EnglishTutor.Modules.Users.Contracts.ReadModels;

namespace EnglishTutor.Modules.Users.Contracts.Readers;

public interface IUserProfileReader
{
    Task<UserProfileReadModel?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken);
}
