using EnglishTutor.BuildingBlocks.Application.Abstractions;
using EnglishTutor.BuildingBlocks.Application.Results;
using EnglishTutor.BuildingBlocks.SharedKernel;
using EnglishTutor.Modules.Vocabulary.Application.Abstractions;
using EnglishTutor.Modules.Vocabulary.Application.DTOs;
using EnglishTutor.Modules.Vocabulary.Application.Errors;
using EnglishTutor.Modules.Vocabulary.Domain.Entities;

namespace EnglishTutor.Modules.Vocabulary.Application.Commands.SubmitExamplePronunciation;

public sealed class SubmitExamplePronunciationCommandHandler(
    IVocabularyItemRepository vocabularyItemRepository,
    IPronunciationAttemptRepository pronunciationAttemptRepository,
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

        var attempt = ExampleSentencePronunciationAttempt.Create(
            request.UserId,
            request.VocabularyExampleId,
            LanguageCode.Create(request.TargetLanguageCode),
            request.RecognizedText,
            request.PronunciationScore,
            request.AccuracyScore,
            request.FluencyScore,
            request.Feedback);

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
