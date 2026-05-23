using EnglishTutor.BuildingBlocks.Application.Abstractions;
using EnglishTutor.Modules.Assessments.Application.Shared.DTOs;

namespace EnglishTutor.Modules.Assessments.Application.Queries.GetAttemptResult;

public sealed record GetAttemptResultQuery(Guid UserId, Guid AttemptId) : IQuery<AttemptResultResponse>;
