using System.Text.RegularExpressions;
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

        var examples = item.Examples.Select(example =>
        {
            var translations = example.Translations
                .Where(t => t.LanguageCode.Value == request.NativeLanguageCode)
                .Select(t => t.Translation)
                .ToList();

            var fillBlank = GenerateFillBlank(example.Sentence, item.Word);

            return new StudyCardExampleResponse(
                example.Id,
                example.Sentence,
                fillBlank,
                item.Word,
                translations);
        }).ToList();

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
            examples,
            mastery?.Status.ToString() ?? "New",
            mastery?.MeaningMasteryScore ?? 0,
            mastery?.PronunciationMasteryScore ?? 0,
            mastery?.ExampleSentenceScore ?? 0,
            mastery?.ReviewCount ?? 0,
            mastery?.ConsecutiveCorrectCount ?? 0,
            mastery?.CanMarkMastered ?? false);
    }

    private static string? GenerateFillBlank(string sentence, string word)
    {
        var pattern = $@"\b{Regex.Escape(word)}\b";
        var match = Regex.Match(sentence, pattern, RegexOptions.IgnoreCase);
        if (!match.Success)
        {
            return null;
        }

        return sentence[..match.Index] + "_____" + sentence[(match.Index + match.Length)..];
    }
}
