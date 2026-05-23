using EnglishTutor.BuildingBlocks.Application.Abstractions;
using EnglishTutor.Modules.Mistakes.Application.Shared.DTOs;

namespace EnglishTutor.Modules.Mistakes.Application.Queries.GetMistake;

public sealed record GetMistakeQuery(Guid UserId, Guid MistakeId) : IQuery<MistakeResponse>;
