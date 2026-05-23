using EnglishTutor.BuildingBlocks.Application.Abstractions;
using EnglishTutor.BuildingBlocks.Application.Results;
using EnglishTutor.BuildingBlocks.SharedKernel;
using EnglishTutor.Modules.Vocabulary.Application.Abstractions;
using EnglishTutor.Modules.Vocabulary.Application.Shared.DTOs;
using EnglishTutor.Modules.Vocabulary.Application.Shared.Errors;
using EnglishTutor.Modules.Vocabulary.Domain.Entities;

namespace EnglishTutor.Modules.Vocabulary.Application.Commands.SubmitExamplePronunciation;

public sealed class SubmitExamplePronunciationCommandHandler(
    IVocabularyItemRepository vocabularyItemRepository,
    IUserVocabularyMasteryRepository masteryRepository,
    IPronunciationAttemptRepository pronunciationAttemptRepository,
    IDateTimeProvider dateTimeProvider,
    IVocabularyUnitOfWork unitOfWork)
    : ICommandHandler<SubmitExamplePronunciationCommand, PronunciationAttemptResponse>
{
    public async Task<Result<PronunciationAttemptResponse>> Handle(SubmitExamplePronunciationCommand request, CancellationToken cancellationToken)
    {
        var example = await vocabularyItemRepository.GetExampleByIdAsync(request.VocabularyExampleId, cancellationToken);
        if (example is null)
        {
            return Result.Failure<PronunciationAttemptResponse>(VocabularyErrors.VocabularyExampleNotFound(request.VocabularyExampleId));
        }

        if (example.TargetLanguageCode.Value != request.TargetLanguageCode)
        {
            return Result.Failure<PronunciationAttemptResponse>(VocabularyErrors.TargetLanguageMismatch);
        }

        var utcNow = dateTimeProvider.UtcNow;
        var attempt = ExampleSentencePronunciationAttempt.Create(
            request.UserId,
            request.VocabularyExampleId,
            LanguageCode.Create(request.TargetLanguageCode),
            request.RecognizedText,
            request.PronunciationScore,
            request.AccuracyScore,
            request.FluencyScore,
            request.Feedback,
            utcNow);

        var mastery = await masteryRepository.GetAsync(
            request.UserId,
            example.VocabularyItemId,
            request.TargetLanguageCode,
            cancellationToken);

        if (mastery is null)
        {
            mastery = UserVocabularyMastery.Create(
                request.UserId,
                example.VocabularyItemId,
                LanguageCode.Create(request.TargetLanguageCode),
                utcNow);

            await masteryRepository.AddAsync(mastery, cancellationToken);
        }

        mastery.RecordPronunciationScore(mastery.PronunciationMasteryScore, request.PronunciationScore);

        await pronunciationAttemptRepository.AddExampleAttemptAsync(attempt, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new PronunciationAttemptResponse(
            attempt.Id,
            attempt.PronunciationScore,
            attempt.AccuracyScore,
            attempt.FluencyScore,
            null,
            attempt.AttemptedAtUtc);
    }
}
