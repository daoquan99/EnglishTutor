using EnglishTutor.BuildingBlocks.Application.Abstractions;
using EnglishTutor.Modules.Speaking.Application.DTOs;

namespace EnglishTutor.Modules.Speaking.Application.Queries.GetSessionSummary;

public sealed record GetSessionSummaryQuery(Guid UserId, Guid SessionId) : IQuery<SpeakingSessionSummaryResponse>;
