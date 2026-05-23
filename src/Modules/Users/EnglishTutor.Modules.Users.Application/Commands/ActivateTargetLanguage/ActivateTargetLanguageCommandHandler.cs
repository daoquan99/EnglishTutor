using EnglishTutor.BuildingBlocks.Application.Abstractions;
using EnglishTutor.BuildingBlocks.Application.Results;
using EnglishTutor.Modules.Users.Application.Abstractions;
using EnglishTutor.Modules.Users.Application.Shared.DTOs;
using EnglishTutor.Modules.Users.Application.Shared.Errors;

namespace EnglishTutor.Modules.Users.Application.Commands.ActivateTargetLanguage;

public sealed class ActivateTargetLanguageCommandHandler(
    IUserTargetLanguageRepository userTargetLanguageRepository,
    IUserLanguageSettingsRepository userLanguageSettingsRepository,
    IDateTimeProvider dateTimeProvider,
    IUsersUnitOfWork unitOfWork)
    : ICommandHandler<ActivateTargetLanguageCommand, TargetLanguageResponse>
{
    public async Task<Result<TargetLanguageResponse>> Handle(ActivateTargetLanguageCommand request, CancellationToken cancellationToken)
    {
        var targetLanguages = await userTargetLanguageRepository.GetByUserIdAsync(request.UserId, cancellationToken);
        var targetLanguage = targetLanguages.SingleOrDefault(language => language.Id == request.TargetLanguageId);
        if (targetLanguage is null)
        {
            return Result.Failure<TargetLanguageResponse>(UserErrors.TargetLanguageNotFound(request.TargetLanguageId));
        }

        var utcNow = dateTimeProvider.UtcNow;
        targetLanguage.Activate(targetLanguages, utcNow);

        var settings = await userLanguageSettingsRepository.GetByUserIdAsync(request.UserId, cancellationToken);
        if (settings is not null)
        {
            settings.Update(
                settings.NativeLanguageCode,
                settings.UiLanguageCode,
                settings.ExplanationLanguageCode,
                targetLanguage.TargetLanguageCode,
                utcNow);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new TargetLanguageResponse(
            targetLanguage.Id,
            targetLanguage.UserId,
            targetLanguage.TargetLanguageCode.Value,
            targetLanguage.CurrentLevel.ToString(),
            targetLanguage.TargetLevel.ToString(),
            targetLanguage.IsActive);
    }
}
