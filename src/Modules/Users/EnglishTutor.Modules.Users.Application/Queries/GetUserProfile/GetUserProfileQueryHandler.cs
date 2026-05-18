using EnglishTutor.BuildingBlocks.Application.Abstractions;
using EnglishTutor.BuildingBlocks.Application.Results;
using EnglishTutor.Modules.Users.Application.Abstractions;
using EnglishTutor.Modules.Users.Application.DTOs;
using EnglishTutor.Modules.Users.Application.Errors;

namespace EnglishTutor.Modules.Users.Application.Queries.GetUserProfile;

public sealed class GetUserProfileQueryHandler(IUserProfileRepository userProfileRepository)
    : IQueryHandler<GetUserProfileQuery, UserProfileResponse>
{
    public async Task<Result<UserProfileResponse>> Handle(GetUserProfileQuery request, CancellationToken cancellationToken)
    {
        var profile = await userProfileRepository.GetByUserIdAsync(request.UserId, cancellationToken);
        if (profile is null)
        {
            return Result.Failure<UserProfileResponse>(UserErrors.ProfileNotFound(request.UserId));
        }

        return new UserProfileResponse(profile.UserId, profile.DisplayName.Value, profile.AvatarUrl, profile.Bio);
    }
}
