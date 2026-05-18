using EnglishTutor.BuildingBlocks.Application.Abstractions;
using EnglishTutor.BuildingBlocks.Application.Results;
using EnglishTutor.Modules.Users.Application.Abstractions;
using EnglishTutor.Modules.Users.Application.DTOs;

namespace EnglishTutor.Modules.Users.Application.Queries.GetTargetLanguages;

public sealed class GetTargetLanguagesQueryHandler(IUserTargetLanguageRepository userTargetLanguageRepository)
    : IQueryHandler<GetTargetLanguagesQuery, IReadOnlyList<TargetLanguageResponse>>
{
    public async Task<Result<IReadOnlyList<TargetLanguageResponse>>> Handle(GetTargetLanguagesQuery request, CancellationToken cancellationToken)
    {
        var targetLanguages = await userTargetLanguageRepository.GetByUserIdAsync(request.UserId, cancellationToken);

        return targetLanguages
            .Select(language => new TargetLanguageResponse(
                language.Id,
                language.UserId,
                language.TargetLanguageCode.Value,
                language.CurrentLevel.ToString(),
                language.TargetLevel.ToString(),
                language.IsActive))
            .ToList();
    }
}
