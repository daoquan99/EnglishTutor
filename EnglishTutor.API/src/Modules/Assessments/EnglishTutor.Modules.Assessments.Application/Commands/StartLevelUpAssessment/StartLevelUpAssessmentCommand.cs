using EnglishTutor.BuildingBlocks.Application.Abstractions;
using EnglishTutor.Modules.Assessments.Application.Shared.DTOs;

namespace EnglishTutor.Modules.Assessments.Application.Commands.StartLevelUpAssessment;

public sealed record StartLevelUpAssessmentCommand(Guid UserId, string? TargetLanguageCode) : ICommand<AttemptDetailResponse>;
