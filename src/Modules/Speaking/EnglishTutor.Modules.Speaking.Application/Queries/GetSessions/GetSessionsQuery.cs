using EnglishTutor.BuildingBlocks.Application.Abstractions;
using EnglishTutor.Modules.Speaking.Application.Shared.DTOs;

namespace EnglishTutor.Modules.Speaking.Application.Queries.GetSessions;

public sealed record GetSessionsQuery(Guid UserId, int Page = 1, int PageSize = 20) : IQuery<IReadOnlyList<SpeakingSessionResponse>>;

