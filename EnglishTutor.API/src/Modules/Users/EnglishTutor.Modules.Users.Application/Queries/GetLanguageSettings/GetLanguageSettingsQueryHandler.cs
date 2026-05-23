using EnglishTutor.BuildingBlocks.Application.Abstractions;
using EnglishTutor.BuildingBlocks.Application.Results;
using EnglishTutor.BuildingBlocks.SharedKernel;
using EnglishTutor.Modules.Users.Application.Abstractions;
using EnglishTutor.Modules.Users.Application.Shared.DTOs;
using EnglishTutor.Modules.Users.Domain.Entities;

namespace EnglishTutor.Modules.Users.Application.Queries.GetLanguageSettings;

public sealed class GetLanguageSettingsQueryHandler(
    IUserLanguageSettingsRepository userLanguageSettingsRepository,
    IUserTargetLanguageRepository userTargetLanguageRepository,
    IUsersUnitOfWork unitOfWork)
    : IQueryHandler<GetLanguageSettingsQuery, LanguageSettingsResponse>
{
    public async Task<Result<LanguageSettingsResponse>> Handle(GetLanguageSettingsQuery request, CancellationToken cancellationToken)
    {
        var settings = await userLanguageSettingsRepository.GetByUserIdAsync(request.UserId, cancellationToken);
        if (settings is null)
        {
            var utcNow = DateTime.UtcNow;
            settings = UserLanguageSettings.CreateDefault(request.UserId, utcNow);
            var targetLanguage = UserTargetLanguage.CreateActive(
                request.UserId,
                LanguageCode.English,
                LanguageLevel.A1,
                LanguageLevel.B2,
                [],
                utcNow);

            await userLanguageSettingsRepository.AddAsync(settings, cancellationToken);
            await userTargetLanguageRepository.AddAsync(targetLanguage, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }

        return new LanguageSettingsResponse(
            settings.UserId,
            settings.NativeLanguageCode.Value,
            settings.UiLanguageCode.Value,
            settings.ExplanationLanguageCode.Value,
            settings.ActiveTargetLanguageCode.Value);
    }
}
