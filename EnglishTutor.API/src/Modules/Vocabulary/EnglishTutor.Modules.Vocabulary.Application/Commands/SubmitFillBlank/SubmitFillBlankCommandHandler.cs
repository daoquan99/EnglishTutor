using EnglishTutor.BuildingBlocks.Application.Abstractions;
using EnglishTutor.BuildingBlocks.Application.Results;
using EnglishTutor.BuildingBlocks.SharedKernel;
using EnglishTutor.Modules.Vocabulary.Application.Abstractions;
using EnglishTutor.Modules.Vocabulary.Application.Shared.Errors;
using EnglishTutor.Modules.Vocabulary.Domain.Entities;

namespace EnglishTutor.Modules.Vocabulary.Application.Commands.SubmitFillBlank;

public sealed class SubmitFillBlankCommandHandler(
    IVocabularyItemRepository vocabularyItemRepository,
    IUserVocabularyMasteryRepository masteryRepository,
    IExampleFillBlankAttemptRepository fillBlankAttemptRepository,
    IDateTimeProvider dateTimeProvider,
    IVocabularyUnitOfWork unitOfWork)
    : ICommandHandler<SubmitFillBlankCommand, FillBlankResultResponse>
{
    public async Task<Result<FillBlankResultResponse>> Handle(SubmitFillBlankCommand request, CancellationToken cancellationToken)
    {
        var example = await vocabularyItemRepository.GetExampleByIdAsync(request.VocabularyExampleId, cancellationToken);
        if (example is null)
        {
            return Result.Failure<FillBlankResultResponse>(VocabularyErrors.VocabularyExampleNotFound(request.VocabularyExampleId));
        }

        var item = await vocabularyItemRepository.GetByIdAsync(example.VocabularyItemId, cancellationToken);
        if (item is null)
        {
            return Result.Failure<FillBlankResultResponse>(VocabularyErrors.VocabularyItemNotFound(example.VocabularyItemId));
        }

        var utcNow = dateTimeProvider.UtcNow;

        var attempt = ExampleFillBlankAttempt.Create(
            request.UserId,
            item.Id,
            example.Id,
            LanguageCode.Create(request.TargetLanguageCode),
            request.UserAnswer,
            item.Word,
            utcNow);

        await fillBlankAttemptRepository.AddAsync(attempt, cancellationToken);

        var mastery = await masteryRepository.GetAsync(
            request.UserId,
            item.Id,
            request.TargetLanguageCode,
            cancellationToken);

        if (mastery is null)
        {
            mastery = UserVocabularyMastery.Create(request.UserId, item.Id, LanguageCode.Create(request.TargetLanguageCode), utcNow);
            await masteryRepository.AddAsync(mastery, cancellationToken);
        }

        mastery.RecordExampleAttempt(attempt.Score, utcNow);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new FillBlankResultResponse(
            attempt.Id,
            attempt.IsCorrect,
            item.Word,
            example.Sentence,
            mastery.ExampleSentenceScore);
    }
}
