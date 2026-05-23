using EnglishTutor.BuildingBlocks.Application.Abstractions;
using EnglishTutor.Modules.StudyPlans.Application.Shared.DTOs;
using EnglishTutor.Modules.StudyPlans.Domain.Enums;

namespace EnglishTutor.Modules.StudyPlans.Application.Queries.GetPlannedSessions;

public sealed record GetPlannedSessionsQuery(
    Guid UserId,
    DateTime? FromDateUtc,
    DateTime? ToDateUtc,
    PlannedSessionStatus? Status) : IQuery<IReadOnlyList<PlannedSessionResponse>>;
