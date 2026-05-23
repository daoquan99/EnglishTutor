using EnglishTutor.Modules.Users.Domain.Entities;

namespace EnglishTutor.Modules.Users.Application.Abstractions;

public interface IUserProfileRepository
{
    Task<UserProfile?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken);

    Task AddAsync(UserProfile profile, CancellationToken cancellationToken);
}
