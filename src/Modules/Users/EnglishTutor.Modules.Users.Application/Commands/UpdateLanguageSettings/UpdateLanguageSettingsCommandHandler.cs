using EnglishTutor.BuildingBlocks.Application.Abstractions;
using EnglishTutor.BuildingBlocks.Application.Results;
using EnglishTutor.BuildingBlocks.SharedKernel;
using EnglishTutor.Modules.Users.Application.Abstractions;
using EnglishTutor.Modules.Users.Application.Shared.DTOs;
using EnglishTutor.Modules.Users.Application.Shared.Errors;

namespace EnglishTutor.Modules.Users.Application.Commands.UpdateLanguageSettings;

public sealed class UpdateLanguageSettingsCommandHandler(
    IUserLanguageSettingsRepository userLanguageSettingsRepository,
    IUserTargetLanguageRepository userTargetLanguageRepository,
    IDateTimeProvider dateTimeProvider,
    IUsersUnitOfWork unitOfWork)
    : ICommandHandler<UpdateLanguageSettingsCommand, LanguageSettingsResponse>
{
    public async Task<Result<LanguageSettingsResponse>> Handle(UpdateLanguageSettingsCommand request, CancellationToken cancellationToken)
    {
        var settings = await userLanguageSettingsRepository.GetByUserIdAsync(request.UserId, cancellationToken);
        if (settings is null)
        {
            return Result.Failure<LanguageSettingsResponse>(UserErrors.LanguageSettingsNotFound(request.UserId));
        }

        var targetLanguages = await userTargetLanguageRepository.GetByUserIdAsync(request.UserId, cancellationToken);
        if (!targetLanguages.Any(language => language.TargetLanguageCode.Value == request.ActiveTargetLanguageCode))
        {
            return Result.Failure<LanguageSettingsResponse>(UserErrors.TargetLanguageNotFound(request.ActiveTargetLanguageCode));
        }

        settings.Update(
            LanguageCode.Create(request.NativeLanguageCode),
            LanguageCode.Create(request.UiLanguageCode),
            LanguageCode.Create(request.ExplanationLanguageCode),
            LanguageCode.Create(request.ActiveTargetLanguageCode),
            dateTimeProvider.UtcNow);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new LanguageSettingsResponse(
            settings.UserId,
            settings.NativeLanguageCode.Value,
            settings.UiLanguageCode.Value,
            settings.ExplanationLanguageCode.Value,
            settings.ActiveTargetLanguageCode.Value);
    }
}
