using EnglishTutor.BuildingBlocks.Application.Abstractions;
using EnglishTutor.Modules.Mistakes.Application.Shared.DTOs;

namespace EnglishTutor.Modules.Mistakes.Application.Queries.GetTodayMistakes;

public sealed record GetTodayMistakesQuery(Guid UserId) : IQuery<IReadOnlyList<MistakeResponse>>;
