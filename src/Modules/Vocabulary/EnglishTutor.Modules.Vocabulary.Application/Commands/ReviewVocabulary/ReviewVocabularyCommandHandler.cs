using EnglishTutor.BuildingBlocks.Application.Abstractions;
using EnglishTutor.BuildingBlocks.Application.Results;
using EnglishTutor.BuildingBlocks.SharedKernel;
using EnglishTutor.Modules.Vocabulary.Application.Abstractions;
using EnglishTutor.Modules.Vocabulary.Application.DTOs;
using EnglishTutor.Modules.Vocabulary.Application.Errors;
using EnglishTutor.Modules.Vocabulary.Domain.Entities;

namespace EnglishTutor.Modules.Vocabulary.Application.Commands.ReviewVocabulary;

public sealed class ReviewVocabularyCommandHandler(
    IVocabularyItemRepository vocabularyItemRepository,
    IUserVocabularyMasteryRepository masteryRepository,
    IVocabularyReviewRepository reviewRepository,
    IVocabularyUnitOfWork unitOfWork)
    : ICommandHandler<ReviewVocabularyCommand, ReviewResultResponse>
{
    public async Task<Result<ReviewResultResponse>> Handle(ReviewVocabularyCommand request, CancellationToken cancellationToken)
    {
        var item = await vocabularyItemRepository.GetByIdAsync(request.VocabularyItemId, cancellationToken);
        if (item is null)
        {
            return Result.Failure<ReviewResultResponse>(VocabularyErrors.VocabularyItemNotFound(request.VocabularyItemId));
        }

        var mastery = await masteryRepository.GetAsync(
            request.UserId,
            request.VocabularyItemId,
            request.TargetLanguageCode,
            cancellationToken);

        if (mastery is null)
        {
            mastery = UserVocabularyMastery.Create(request.UserId, request.VocabularyItemId, LanguageCode.Create(request.TargetLanguageCode));
            await masteryRepository.AddAsync(mastery, cancellationToken);
        }

        mastery.RecordReview(request.IsCorrect, request.Score);
        await reviewRepository.AddAttemptAsync(
            VocabularyReviewAttempt.Create(
                request.UserId,
                request.VocabularyItemId,
                LanguageCode.Create(request.TargetLanguageCode),
                request.IsCorrect,
                request.Score),
            cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new ReviewResultResponse(
            mastery.VocabularyItemId,
            mastery.Status.ToString(),
            mastery.MeaningMasteryScore,
            mastery.NextReviewAtUtc);
    }
}
