using EnglishTutor.BuildingBlocks.Application.Abstractions;
using EnglishTutor.Modules.Speaking.Application.DTOs;

namespace EnglishTutor.Modules.Speaking.Application.Queries.GetSession;

public sealed record GetSessionQuery(Guid UserId, Guid SessionId) : IQuery<SpeakingSessionResponse>;
