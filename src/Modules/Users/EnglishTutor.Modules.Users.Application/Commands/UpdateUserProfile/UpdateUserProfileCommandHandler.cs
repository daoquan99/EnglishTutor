using EnglishTutor.BuildingBlocks.Application.Abstractions;
using EnglishTutor.BuildingBlocks.Application.Results;
using EnglishTutor.Modules.Users.Application.Abstractions;
using EnglishTutor.Modules.Users.Application.Shared.DTOs;
using EnglishTutor.Modules.Users.Application.Shared.Errors;
using EnglishTutor.Modules.Users.Domain.Entities;

namespace EnglishTutor.Modules.Users.Application.Commands.UpdateUserProfile;

public sealed class UpdateUserProfileCommandHandler(
    IUserProfileRepository userProfileRepository,
    IDateTimeProvider dateTimeProvider,
    IUsersUnitOfWork unitOfWork)
    : ICommandHandler<UpdateUserProfileCommand, UserProfileResponse>
{
    public async Task<Result<UserProfileResponse>> Handle(UpdateUserProfileCommand request, CancellationToken cancellationToken)
    {
        var profile = await userProfileRepository.GetByUserIdAsync(request.UserId, cancellationToken);
        if (profile is null)
        {
            profile = UserProfile.Create(request.UserId, request.DisplayName, dateTimeProvider.UtcNow);
            await userProfileRepository.AddAsync(profile, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            return new UserProfileResponse(profile.UserId, profile.DisplayName.Value, profile.AvatarUrl, profile.Bio);
        }

        profile.UpdateProfile(request.DisplayName, request.AvatarUrl, request.Bio, dateTimeProvider.UtcNow);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new UserProfileResponse(profile.UserId, profile.DisplayName.Value, profile.AvatarUrl, profile.Bio);
    }
}
