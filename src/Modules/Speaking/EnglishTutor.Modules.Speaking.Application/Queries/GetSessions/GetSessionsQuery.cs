using EnglishTutor.BuildingBlocks.Application.Abstractions;
using EnglishTutor.Modules.Speaking.Application.DTOs;

namespace EnglishTutor.Modules.Speaking.Application.Queries.GetSessions;

public sealed record GetSessionsQuery(Guid UserId) : IQuery<IReadOnlyList<SpeakingSessionResponse>>;
