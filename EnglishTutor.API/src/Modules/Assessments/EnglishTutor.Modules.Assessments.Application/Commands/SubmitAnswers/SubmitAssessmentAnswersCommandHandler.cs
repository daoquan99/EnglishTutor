using EnglishTutor.BuildingBlocks.Application.Abstractions;
using EnglishTutor.BuildingBlocks.Application.Results;
using EnglishTutor.Modules.Assessments.Application.Abstractions;
using EnglishTutor.Modules.Assessments.Application.Shared.DTOs;
using EnglishTutor.Modules.Assessments.Application.Shared.Errors;
using EnglishTutor.Modules.Assessments.Application.Shared.Mappers;
using EnglishTutor.Modules.Assessments.Domain.UserAssessmentAttempt.Enums;

namespace EnglishTutor.Modules.Assessments.Application.Commands.SubmitAnswers;

public sealed class SubmitAssessmentAnswersCommandHandler(
    IAssessmentAttemptRepository attemptRepository,
    IAssessmentDefinitionRepository definitionRepository,
    IAssessmentsUnitOfWork unitOfWork,
    IDateTimeProvider dateTimeProvider)
    : ICommandHandler<SubmitAssessmentAnswersCommand, AttemptDetailResponse>
{
    public async Task<Result<AttemptDetailResponse>> Handle(SubmitAssessmentAnswersCommand request, CancellationToken cancellationToken)
    {
        var attempt = await attemptRepository.GetByIdWithAnswersAsync(request.AttemptId, cancellationToken);
        if (attempt is null)
        {
            return Result.Failure<AttemptDetailResponse>(AssessmentErrors.AttemptNotFound(request.AttemptId));
        }

        if (attempt.UserId != request.UserId)
        {
            return Result.Failure<AttemptDetailResponse>(AssessmentErrors.AttemptNotOwned);
        }

        if (attempt.Status != AssessmentAttemptStatus.InProgress)
        {
            return Result.Failure<AttemptDetailResponse>(AssessmentErrors.AttemptAlreadySubmitted);
        }

        var definition = await definitionRepository.GetByIdWithDetailsAsync(attempt.AssessmentDefinitionId, cancellationToken);
        if (definition is null)
        {
            return Result.Failure<AttemptDetailResponse>(AssessmentErrors.DefinitionNotFound);
        }

        var questionIds = definition.Sections.SelectMany(section => section.Questions).Select(question => question.Id).ToHashSet();
        var existingAnswerIds = attempt.Answers.Select(answer => answer.Id).ToHashSet();
        foreach (var answer in request.Answers)
        {
            if (!questionIds.Contains(answer.QuestionId) || attempt.Answers.Any(existing => existing.QuestionId == answer.QuestionId))
            {
                continue;
            }

            attempt.SubmitAnswer(answer.QuestionId, answer.UserAnswer, dateTimeProvider.UtcNow);
        }

        foreach (var answer in attempt.Answers.Where(answer => !existingAnswerIds.Contains(answer.Id)))
        {
            await attemptRepository.AddAnswerAsync(answer, cancellationToken);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return attempt.ToDetailResponse(definition);
    }
}
