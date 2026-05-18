using EnglishTutor.BuildingBlocks.Application.Abstractions;
using EnglishTutor.BuildingBlocks.Application.Results;
using EnglishTutor.Modules.Users.Application.Abstractions;
using EnglishTutor.Modules.Users.Domain.Entities;

namespace EnglishTutor.Modules.Users.Application.Commands.CreateUserProfile;

public sealed class CreateUserProfileCommandHandler(
    IUserProfileRepository userProfileRepository,
    IUserLanguageSettingsRepository userLanguageSettingsRepository,
    IUserTargetLanguageRepository userTargetLanguageRepository,
    IUsersUnitOfWork unitOfWork)
    : ICommandHandler<CreateUserProfileCommand>
{
    public async Task<Result> Handle(CreateUserProfileCommand request, CancellationToken cancellationToken)
    {
        var profile = UserProfile.Create(request.UserId, request.DisplayName);
        var settings = UserLanguageSettings.CreateDefault(request.UserId);
        var targetLanguage = UserTargetLanguage.CreateActive(
            request.UserId,
            BuildingBlocks.SharedKernel.LanguageCode.English,
            BuildingBlocks.SharedKernel.LanguageLevel.A1,
            BuildingBlocks.SharedKernel.LanguageLevel.B2,
            []);

        await userProfileRepository.AddAsync(profile, cancellationToken);
        await userLanguageSettingsRepository.AddAsync(settings, cancellationToken);
        await userTargetLanguageRepository.AddAsync(targetLanguage, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
