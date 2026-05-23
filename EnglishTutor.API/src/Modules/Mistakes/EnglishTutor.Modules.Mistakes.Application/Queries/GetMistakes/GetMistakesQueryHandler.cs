using EnglishTutor.BuildingBlocks.Application.Abstractions;
using EnglishTutor.BuildingBlocks.Application.Results;
using EnglishTutor.Modules.Mistakes.Application.Abstractions;
using EnglishTutor.Modules.Mistakes.Application.Shared.DTOs;

namespace EnglishTutor.Modules.Mistakes.Application.Queries.GetMistakes;

public sealed class GetMistakesQueryHandler(IMistakeRepository mistakeRepository)
    : IQueryHandler<GetMistakesQuery, IReadOnlyList<MistakeResponse>>
{
    public async Task<Result<IReadOnlyList<MistakeResponse>>> Handle(GetMistakesQuery request, CancellationToken cancellationToken)
    {
        var mistakes = await mistakeRepository.GetByUserIdAsync(request.UserId, cancellationToken);
        return mistakes.Select(MistakeMappers.ToResponse).ToList();
    }
}
