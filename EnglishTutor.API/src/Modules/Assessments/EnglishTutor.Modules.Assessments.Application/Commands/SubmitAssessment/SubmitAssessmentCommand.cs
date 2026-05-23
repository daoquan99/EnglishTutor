using EnglishTutor.BuildingBlocks.Application.Abstractions;
using EnglishTutor.Modules.Assessments.Application.Shared.DTOs;

namespace EnglishTutor.Modules.Assessments.Application.Commands.SubmitAssessment;

public sealed record SubmitAssessmentCommand(Guid UserId, Guid AttemptId) : ICommand<AttemptResultResponse>;
