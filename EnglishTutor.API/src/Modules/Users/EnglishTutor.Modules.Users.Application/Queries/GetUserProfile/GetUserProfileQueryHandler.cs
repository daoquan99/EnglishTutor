using EnglishTutor.BuildingBlocks.Application.Abstractions;
using EnglishTutor.BuildingBlocks.Application.Results;
using EnglishTutor.Modules.Users.Application.Abstractions;
using EnglishTutor.Modules.Users.Application.Shared.DTOs;
using EnglishTutor.Modules.Users.Domain.Entities;

namespace EnglishTutor.Modules.Users.Application.Queries.GetUserProfile;

public sealed class GetUserProfileQueryHandler(
    IUserProfileRepository userProfileRepository,
    ICurrentUser currentUser,
    IUsersUnitOfWork unitOfWork)
    : IQueryHandler<GetUserProfileQuery, UserProfileResponse>
{
    public async Task<Result<UserProfileResponse>> Handle(GetUserProfileQuery request, CancellationToken cancellationToken)
    {
        var profile = await userProfileRepository.GetByUserIdAsync(request.UserId, cancellationToken);
        if (profile is null)
        {
            var displayName = DeriveDisplayName(currentUser.Email);
            profile = UserProfile.Create(request.UserId, displayName, DateTime.UtcNow);
            await userProfileRepository.AddAsync(profile, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }

        return new UserProfileResponse(profile.UserId, profile.DisplayName.Value, profile.AvatarUrl, profile.Bio);
    }

    private static string DeriveDisplayName(string? email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return "User";
        }

        var prefix = email.Split('@')[0];
        if (string.IsNullOrWhiteSpace(prefix) || prefix.Length < 2)
        {
            return "User";
        }

        return char.ToUpperInvariant(prefix[0]) + prefix[1..];
    }
}
