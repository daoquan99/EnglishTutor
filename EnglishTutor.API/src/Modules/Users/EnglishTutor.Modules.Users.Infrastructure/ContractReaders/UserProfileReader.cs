using EnglishTutor.Modules.Users.Contracts.Readers;
using EnglishTutor.Modules.Users.Contracts.ReadModels;
using EnglishTutor.Modules.Users.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EnglishTutor.Modules.Users.Infrastructure.ContractReaders;

public sealed class UserProfileReader(UsersDbContext dbContext) : IUserProfileReader
{
    public Task<UserProfileReadModel?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken) =>
        dbContext.UserProfiles
            .Where(profile => profile.UserId == userId)
            .Select(profile => new UserProfileReadModel(
                profile.UserId,
                profile.DisplayName.Value,
                profile.AvatarUrl,
                profile.Bio))
            .SingleOrDefaultAsync(cancellationToken);
}
