using EnglishTutor.BuildingBlocks.Application.Abstractions;
using EnglishTutor.Modules.Assessments.Application.Shared.DTOs;

namespace EnglishTutor.Modules.Assessments.Application.Commands.SubmitAnswers;

public sealed record SubmitAssessmentAnswersCommand(
    Guid UserId,
    Guid AttemptId,
    IReadOnlyList<AnswerSubmission> Answers) : ICommand<AttemptDetailResponse>;
