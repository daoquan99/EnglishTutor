using EnglishTutor.BuildingBlocks.Application.Abstractions;
using EnglishTutor.Modules.Assessments.Application.Shared.DTOs;

namespace EnglishTutor.Modules.Assessments.Application.Queries.GetAttempt;

public sealed record GetAttemptQuery(Guid UserId, Guid AttemptId) : IQuery<AttemptDetailResponse>;
