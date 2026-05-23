using EnglishTutor.BuildingBlocks.Application.Abstractions;
using EnglishTutor.BuildingBlocks.Application.Results;
using EnglishTutor.BuildingBlocks.SharedKernel;
using EnglishTutor.Modules.Users.Application.Abstractions;
using EnglishTutor.Modules.Users.Application.Shared.DTOs;
using EnglishTutor.Modules.Users.Application.Shared.Errors;
using EnglishTutor.Modules.Users.Domain.Entities;

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
        var targetLanguages = await userTargetLanguageRepository.GetByUserIdAsync(request.UserId, cancellationToken);
        if (!targetLanguages.Any(language => language.TargetLanguageCode.Value == request.ActiveTargetLanguageCode))
        {
            return Result.Failure<LanguageSettingsResponse>(UserErrors.TargetLanguageNotFound(request.ActiveTargetLanguageCode));
        }

        var settings = await userLanguageSettingsRepository.GetByUserIdAsync(request.UserId, cancellationToken);
        var isNew = settings is null;
        if (isNew)
        {
            settings = UserLanguageSettings.CreateDefault(request.UserId, dateTimeProvider.UtcNow);
        }

        settings!.Update(
            LanguageCode.Create(request.NativeLanguageCode),
            LanguageCode.Create(request.UiLanguageCode),
            LanguageCode.Create(request.ExplanationLanguageCode),
            LanguageCode.Create(request.ActiveTargetLanguageCode),
            dateTimeProvider.UtcNow);

        if (isNew)
        {
            await userLanguageSettingsRepository.AddAsync(settings, cancellationToken);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new LanguageSettingsResponse(
            settings.UserId,
            settings.NativeLanguageCode.Value,
            settings.UiLanguageCode.Value,
            settings.ExplanationLanguageCode.Value,
            settings.ActiveTargetLanguageCode.Value);
    }
}
