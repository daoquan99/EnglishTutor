using EnglishTutor.BuildingBlocks.Application.Abstractions;
using EnglishTutor.BuildingBlocks.Application.Results;
using EnglishTutor.Modules.Vocabulary.Application.Abstractions;
using EnglishTutor.Modules.Vocabulary.Application.DTOs;
using EnglishTutor.Modules.Vocabulary.Application.Errors;

namespace EnglishTutor.Modules.Vocabulary.Application.Commands.MarkMastered;

public sealed class MarkMasteredCommandHandler(
    IUserVocabularyMasteryRepository masteryRepository,
    IVocabularyUnitOfWork unitOfWork)
    : ICommandHandler<MarkMasteredCommand, ReviewResultResponse>
{
    public async Task<Result<ReviewResultResponse>> Handle(MarkMasteredCommand request, CancellationToken cancellationToken)
    {
        var mastery = await masteryRepository.GetAsync(request.UserId, request.VocabularyItemId, request.TargetLanguageCode, cancellationToken);
        if (mastery is null)
        {
            return Result.Failure<ReviewResultResponse>(VocabularyErrors.MasteryNotFound);
        }

        mastery.MarkMastered();
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new ReviewResultResponse(
            mastery.VocabularyItemId,
            mastery.Status.ToString(),
            mastery.MeaningMasteryScore,
            mastery.NextReviewAtUtc);
    }
}
