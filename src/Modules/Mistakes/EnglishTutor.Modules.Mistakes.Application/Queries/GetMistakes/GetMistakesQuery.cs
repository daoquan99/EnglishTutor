using EnglishTutor.BuildingBlocks.Application.Abstractions;
using EnglishTutor.Modules.Mistakes.Application.DTOs;

namespace EnglishTutor.Modules.Mistakes.Application.Queries.GetMistakes;

public sealed record GetMistakesQuery(Guid UserId) : IQuery<IReadOnlyList<MistakeResponse>>;
