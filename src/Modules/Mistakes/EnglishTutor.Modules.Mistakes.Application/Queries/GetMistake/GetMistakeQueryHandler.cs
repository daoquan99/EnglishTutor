using EnglishTutor.BuildingBlocks.Application.Abstractions;
using EnglishTutor.BuildingBlocks.Application.Results;
using EnglishTutor.Modules.Mistakes.Application.Abstractions;
using EnglishTutor.Modules.Mistakes.Application.Shared.DTOs;
using EnglishTutor.Modules.Mistakes.Application.Shared.Errors;

namespace EnglishTutor.Modules.Mistakes.Application.Queries.GetMistake;

public sealed class GetMistakeQueryHandler(IMistakeRepository mistakeRepository)
    : IQueryHandler<GetMistakeQuery, MistakeResponse>
{
    public async Task<Result<MistakeResponse>> Handle(GetMistakeQuery request, CancellationToken cancellationToken)
    {
        var mistake = await mistakeRepository.GetByIdAsync(request.MistakeId, cancellationToken);
        if (mistake is null || mistake.UserId != request.UserId)
        {
            return Result.Failure<MistakeResponse>(MistakeErrors.MistakeNotFound(request.MistakeId));
        }

        return MistakeMappers.ToResponse(mistake);
    }
}
