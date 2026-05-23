using EnglishTutor.Modules.Users.Application.Abstractions;
using EnglishTutor.Modules.Users.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace EnglishTutor.Modules.Users.Infrastructure.Persistence.Repositories;

public sealed class UserProfileRepository(UsersDbContext dbContext) : IUserProfileRepository
{
    public Task<UserProfile?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken) =>
        dbContext.UserProfiles.SingleOrDefaultAsync(profile => profile.UserId == userId, cancellationToken);

    public Task AddAsync(UserProfile profile, CancellationToken cancellationToken)
    {
        dbContext.UserProfiles.Add(profile);
        return Task.CompletedTask;
    }
}
