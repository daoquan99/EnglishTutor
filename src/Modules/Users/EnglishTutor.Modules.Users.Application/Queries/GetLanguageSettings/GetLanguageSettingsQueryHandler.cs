using EnglishTutor.BuildingBlocks.Application.Abstractions;
using EnglishTutor.BuildingBlocks.Application.Results;
using EnglishTutor.Modules.Users.Application.Abstractions;
using EnglishTutor.Modules.Users.Application.Shared.DTOs;
using EnglishTutor.Modules.Users.Application.Shared.Errors;

namespace EnglishTutor.Modules.Users.Application.Queries.GetLanguageSettings;

public sealed class GetLanguageSettingsQueryHandler(IUserLanguageSettingsRepository userLanguageSettingsRepository)
    : IQueryHandler<GetLanguageSettingsQuery, LanguageSettingsResponse>
{
    public async Task<Result<LanguageSettingsResponse>> Handle(GetLanguageSettingsQuery request, CancellationToken cancellationToken)
    {
        var settings = await userLanguageSettingsRepository.GetByUserIdAsync(request.UserId, cancellationToken);
        if (settings is null)
        {
            return Result.Failure<LanguageSettingsResponse>(UserErrors.LanguageSettingsNotFound(request.UserId));
        }

        return new LanguageSettingsResponse(
            settings.UserId,
            settings.NativeLanguageCode.Value,
            settings.UiLanguageCode.Value,
            settings.ExplanationLanguageCode.Value,
            settings.ActiveTargetLanguageCode.Value);
    }
}
