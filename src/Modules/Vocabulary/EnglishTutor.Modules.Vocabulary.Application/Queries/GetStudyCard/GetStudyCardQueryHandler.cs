using EnglishTutor.BuildingBlocks.Application.Abstractions;
using EnglishTutor.BuildingBlocks.Application.Results;
using EnglishTutor.Modules.Vocabulary.Application.Abstractions;
using EnglishTutor.Modules.Vocabulary.Application.Shared.Errors;

namespace EnglishTutor.Modules.Vocabulary.Application.Queries.GetStudyCard;

public sealed class GetStudyCardQueryHandler(
    IVocabularyItemRepository vocabularyItemRepository,
    IUserVocabularyMasteryRepository masteryRepository)
    : IQueryHandler<GetStudyCardQuery, StudyCardResponse>
{
    public async Task<Result<StudyCardResponse>> Handle(GetStudyCardQuery request, CancellationToken cancellationToken)
    {
        var item = await vocabularyItemRepository.GetByIdAsync(request.VocabularyItemId, cancellationToken);
        if (item is null)
        {
            return Result.Failure<StudyCardResponse>(VocabularyErrors.VocabularyItemNotFound(request.VocabularyItemId));
        }

        if (item.TargetLanguageCode.Value != request.TargetLanguageCode)
        {
            return Result.Failure<StudyCardResponse>(VocabularyErrors.TargetLanguageMismatch);
        }

        var mastery = await masteryRepository.GetAsync(
            request.UserId,
            request.VocabularyItemId,
            request.TargetLanguageCode,
            cancellationToken);

        return new StudyCardResponse(
            item.Id,
            item.Word,
            item.Phonetic,
            item.PartOfSpeech.ToString(),
            item.Topic,
            item.Translations
                .Where(translation => translation.LanguageCode.Value == request.NativeLanguageCode)
                .Select(translation => translation.Meaning)
                .ToList(),
            item.Examples.Select(example => example.Sentence).ToList(),
            mastery?.MeaningMasteryScore ?? 0,
            mastery?.PronunciationMasteryScore ?? 0,
            mastery?.ExampleSentenceScore ?? 0);
    }
}
