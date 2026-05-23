using EnglishTutor.BuildingBlocks.Application.Abstractions;
using EnglishTutor.BuildingBlocks.Application.Results;
using EnglishTutor.Modules.Mistakes.Application.Abstractions;
using EnglishTutor.Modules.Mistakes.Application.Shared.DTOs;
using EnglishTutor.Modules.Mistakes.Application.Shared.Errors;

namespace EnglishTutor.Modules.Mistakes.Application.Commands.MarkMistakeMastered;

public sealed class MarkMistakeMasteredCommandHandler(
    IMistakeRepository mistakeRepository,
    IDateTimeProvider dateTimeProvider,
    IMistakesUnitOfWork unitOfWork)
    : ICommandHandler<MarkMistakeMasteredCommand, MistakeResponse>
{
    public async Task<Result<MistakeResponse>> Handle(MarkMistakeMasteredCommand request, CancellationToken cancellationToken)
    {
        var mistake = await mistakeRepository.GetByIdAsync(request.MistakeId, cancellationToken);
        if (mistake is null || mistake.UserId != request.UserId)
        {
            return Result.Failure<MistakeResponse>(MistakeErrors.MistakeNotFound(request.MistakeId));
        }

        mistake.MarkMastered(dateTimeProvider.UtcNow);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return MistakeMappers.ToResponse(mistake);
    }
}
