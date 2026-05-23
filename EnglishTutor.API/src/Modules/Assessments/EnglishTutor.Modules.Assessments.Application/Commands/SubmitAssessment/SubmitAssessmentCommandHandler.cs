using EnglishTutor.BuildingBlocks.Application.Abstractions;
using EnglishTutor.BuildingBlocks.Application.Results;
using EnglishTutor.BuildingBlocks.SharedKernel;
using EnglishTutor.Modules.Assessments.Application.Abstractions;
using EnglishTutor.Modules.Assessments.Application.Shared.DTOs;
using EnglishTutor.Modules.Assessments.Application.Shared.Errors;
using EnglishTutor.Modules.Assessments.Domain.AssessmentDefinition;
using EnglishTutor.Modules.Assessments.Domain.AssessmentDefinition.Entities;
using EnglishTutor.Modules.Assessments.Domain.UserAssessmentAttempt;
using EnglishTutor.Modules.Assessments.Domain.UserAssessmentAttempt.Entities;
using EnglishTutor.Modules.Assessments.Domain.UserAssessmentAttempt.Enums;

namespace EnglishTutor.Modules.Assessments.Application.Commands.SubmitAssessment;

public sealed class SubmitAssessmentCommandHandler(
    IAssessmentAttemptRepository attemptRepository,
    IAssessmentDefinitionRepository definitionRepository,
    AssessmentGradingOrchestrator gradingOrchestrator,
    IAssessmentsUnitOfWork unitOfWork,
    IDateTimeProvider dateTimeProvider)
    : ICommandHandler<SubmitAssessmentCommand, AttemptResultResponse>
{
    public async Task<Result<AttemptResultResponse>> Handle(SubmitAssessmentCommand request, CancellationToken cancellationToken)
    {
        var attempt = await attemptRepository.GetByIdWithAnswersAsync(request.AttemptId, cancellationToken);
        if (attempt is null)
        {
            return Result.Failure<AttemptResultResponse>(AssessmentErrors.AttemptNotFound(request.AttemptId));
        }

        if (attempt.UserId != request.UserId)
        {
            return Result.Failure<AttemptResultResponse>(AssessmentErrors.AttemptNotOwned);
        }

        if (attempt.Status is AssessmentAttemptStatus.Passed or AssessmentAttemptStatus.Failed)
        {
            return Result.Failure<AttemptResultResponse>(AssessmentErrors.AlreadyGraded);
        }

        var definition = await definitionRepository.GetByIdWithDetailsAsync(attempt.AssessmentDefinitionId, cancellationToken);
        if (definition is null)
        {
            return Result.Failure<AttemptResultResponse>(AssessmentErrors.DefinitionNotFound);
        }

        var expectedAnswers = definition.Sections.Sum(section => section.Questions.Count);
        if (attempt.Answers.Count != expectedAnswers)
        {
            return Result.Failure<AttemptResultResponse>(AssessmentErrors.NotAllQuestionsAnswered);
        }

        var utcNow = dateTimeProvider.UtcNow;
        if (attempt.Status == AssessmentAttemptStatus.InProgress)
        {
            attempt.Submit(expectedAnswers, utcNow);
        }

        attempt.MarkGrading(utcNow);
        var grading = await gradingOrchestrator.GradeAsync(definition, attempt, utcNow, cancellationToken);
        attempt.ApplyGradingResult(grading.SectionScores, grading.TotalScore, definition.PassingScore, definition.MinSkillScore, utcNow);

        var result = AssessmentGradingResult.Create(
            attempt.Id,
            grading.TotalScore,
            grading.SectionScoresJson,
            attempt.Status == AssessmentAttemptStatus.Passed,
            utcNow,
            null);
        await attemptRepository.AddResultAsync(result, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new AttemptResultResponse(
            attempt.Id,
            attempt.Status.ToString(),
            grading.TotalScore,
            attempt.Status == AssessmentAttemptStatus.Passed,
            grading.SectionScores.Select(pair => new SectionScoreResponse(pair.Key.ToString(), pair.Value)).ToArray(),
            attempt.GradedAtUtc);
    }
}
