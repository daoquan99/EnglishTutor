using EnglishTutor.BuildingBlocks.Application.Abstractions;
using EnglishTutor.BuildingBlocks.Application.Results;
using EnglishTutor.BuildingBlocks.SharedKernel;
using EnglishTutor.Modules.Vocabulary.Application.Abstractions;
using EnglishTutor.Modules.Vocabulary.Application.DTOs;
using EnglishTutor.Modules.Vocabulary.Application.Errors;
using EnglishTutor.Modules.Vocabulary.Domain.Entities;

namespace EnglishTutor.Modules.Vocabulary.Application.Commands.SubmitPronunciationAttempt;

public sealed class SubmitPronunciationAttemptCommandHandler(
    IVocabularyItemRepository vocabularyItemRepository,
    IPronunciationAttemptRepository pronunciationAttemptRepository,
    IVocabularyUnitOfWork unitOfWork)
    : ICommandHandler<SubmitPronunciationAttemptCommand, PronunciationAttemptResponse>
{
    public async Task<Result<PronunciationAttemptResponse>> Handle(SubmitPronunciationAttemptCommand request, CancellationToken cancellationToken)
    {
        var item = await vocabularyItemRepository.GetByIdAsync(request.VocabularyItemId, cancellationToken);
        if (item is null)
        {
            return Result.Failure<PronunciationAttemptResponse>(VocabularyErrors.VocabularyItemNotFound(request.VocabularyItemId));
        }

        var attempt = VocabularyPronunciationAttempt.Create(
            request.UserId,
            request.VocabularyItemId,
            LanguageCode.Create(request.TargetLanguageCode),
            request.AudioUrl,
            request.RecognizedText,
            request.PronunciationScore,
            request.AccuracyScore,
            request.FluencyScore,
            request.CompletenessScore,
            request.Feedback);

        await pronunciationAttemptRepository.AddVocabularyAttemptAsync(attempt, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new PronunciationAttemptResponse(
            attempt.Id,
            attempt.PronunciationScore,
            attempt.AccuracyScore,
            attempt.FluencyScore,
            attempt.CompletenessScore,
            attempt.AttemptedAtUtc);
    }
}
