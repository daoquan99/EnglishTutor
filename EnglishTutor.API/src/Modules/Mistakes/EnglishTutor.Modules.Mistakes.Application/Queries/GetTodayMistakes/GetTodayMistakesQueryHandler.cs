using EnglishTutor.BuildingBlocks.Application.Abstractions;
using EnglishTutor.BuildingBlocks.Application.Results;
using EnglishTutor.Modules.Mistakes.Application.Abstractions;
using EnglishTutor.Modules.Mistakes.Application.Shared.DTOs;

namespace EnglishTutor.Modules.Mistakes.Application.Queries.GetTodayMistakes;

public sealed class GetTodayMistakesQueryHandler(
    IMistakeRepository mistakeRepository,
    IDateTimeProvider dateTimeProvider)
    : IQueryHandler<GetTodayMistakesQuery, IReadOnlyList<MistakeResponse>>
{
    public async Task<Result<IReadOnlyList<MistakeResponse>>> Handle(GetTodayMistakesQuery request, CancellationToken cancellationToken)
    {
        var mistakes = await mistakeRepository.GetDueAsync(request.UserId, dateTimeProvider.UtcNow, cancellationToken);
        return mistakes.Select(MistakeMappers.ToResponse).ToList();
    }
}
