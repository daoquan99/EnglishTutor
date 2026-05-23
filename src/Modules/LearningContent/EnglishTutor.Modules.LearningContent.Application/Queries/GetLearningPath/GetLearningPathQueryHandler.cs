using EnglishTutor.BuildingBlocks.Application.Abstractions;
using EnglishTutor.BuildingBlocks.Application.Results;
using EnglishTutor.Modules.LearningContent.Application.Abstractions;
using EnglishTutor.Modules.LearningContent.Application.Shared.DTOs;
using EnglishTutor.Modules.LearningContent.Application.Shared.Mappers;
using EnglishTutor.Modules.Users.Contracts.Readers;

namespace EnglishTutor.Modules.LearningContent.Application.Queries.GetLearningPath;

public sealed class GetLearningPathQueryHandler(
    ILearningPathCardRepository cardRepository,
    IUserLanguageSettingsReader languageSettingsReader)
    : IQueryHandler<GetLearningPathQuery, IReadOnlyList<LearningPathCardResponse>>
{
    public async Task<Result<IReadOnlyList<LearningPathCardResponse>>> Handle(GetLearningPathQuery request, CancellationToken cancellationToken)
    {
        var settings = await languageSettingsReader.GetByUserIdAsync(request.UserId, cancellationToken);
        var targetLanguageCode = request.TargetLanguageCode ?? settings?.TargetLanguageCode ?? "en";
        var cards = await cardRepository.ListAsync(request.UserId, targetLanguageCode, cancellationToken);

        return Result.Success<IReadOnlyList<LearningPathCardResponse>>(cards.Select(card => card.ToResponse()).ToArray());
    }
}
