using EnglishTutor.BuildingBlocks.Application.Abstractions;
using EnglishTutor.Modules.Assessments.Application.Shared.DTOs;

namespace EnglishTutor.Modules.Assessments.Application.Queries.GetAvailableAssessments;

public sealed record GetAvailableAssessmentsQuery(Guid UserId, string? TargetLanguageCode) : IQuery<IReadOnlyList<AvailableAssessmentResponse>>;
