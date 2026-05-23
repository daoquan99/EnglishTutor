using EnglishTutor.BuildingBlocks.Application.Abstractions;
using EnglishTutor.BuildingBlocks.Application.Results;
using EnglishTutor.Modules.AI.Contracts.DTOs;
using EnglishTutor.Modules.AI.Contracts.Services;
using EnglishTutor.Modules.Exercises.Application.Abstractions;
using EnglishTutor.Modules.Exercises.Application.Shared.Errors;
using EnglishTutor.Modules.Exercises.Domain.ExerciseSet.Enums;
using EnglishTutor.Modules.Exercises.Domain.Shared;
using EnglishTutor.Modules.Exercises.Domain.UserExerciseAttempt.Enums;
using EnglishTutor.Modules.Users.Contracts.Readers;

namespace EnglishTutor.Modules.Exercises.Application.Commands.SubmitAnswer;

public sealed class SubmitAnswerCommandHandler(
    IExerciseAttemptRepository exerciseAttemptRepository,
    IExerciseSetRepository exerciseSetRepository,
    IExercisesUnitOfWork unitOfWork,
    ExerciseGradingService gradingService,
    IAssessmentGradingService assessmentGradingService,
    IUserLanguageSettingsReader languageSettingsReader,
    IDateTimeProvider dateTimeProvider)
    : ICommandHandler<SubmitAnswerCommand, SubmitAnswerResponse>
{
    public async Task<Result<SubmitAnswerResponse>> Handle(SubmitAnswerCommand request, CancellationToken cancellationToken)
    {
        var attempt = await exerciseAttemptRepository.GetByIdWithAnswersAsync(request.AttemptId, cancellationToken);
        if (attempt is null)
        {
            return Result.Failure<SubmitAnswerResponse>(ExerciseErrors.AttemptNotFound(request.AttemptId));
        }

        if (attempt.UserId != request.UserId)
        {
            return Result.Failure<SubmitAnswerResponse>(ExerciseErrors.Forbidden);
        }

        if (attempt.Status == ExerciseAttemptStatus.Completed)
        {
            return Result.Failure<SubmitAnswerResponse>(ExerciseErrors.AttemptAlreadyCompleted);
        }

        if (attempt.HasAnswered(request.QuestionId))
        {
            return Result.Failure<SubmitAnswerResponse>(ExerciseErrors.QuestionAlreadyAnswered);
        }

        var exerciseSet = await exerciseSetRepository.GetByIdWithQuestionsAsync(attempt.ExerciseSetId, cancellationToken);
        if (exerciseSet is null)
        {
            return Result.Failure<SubmitAnswerResponse>(ExerciseErrors.ExerciseSetNotFound(attempt.ExerciseSetId));
        }

        var question = exerciseSet.Questions.SingleOrDefault(candidate => candidate.Id == request.QuestionId);
        if (question is null)
        {
            return Result.Failure<SubmitAnswerResponse>(ExerciseErrors.QuestionNotFound(request.QuestionId));
        }

        var grading = question.IsAiGraded
            ? await GradeWithAiAsync(request.UserId, attempt.TargetLanguageCode, exerciseSet.Level.ToString(), question.Prompt, request.UserAnswer, question.CorrectAnswer, cancellationToken)
            : gradingService.GradeStaticAnswer(question, request.UserAnswer);

        var utcNow = dateTimeProvider.UtcNow;
        var answer = attempt.RecordAnswer(
            question.Id,
            request.UserAnswer,
            grading.IsCorrect,
            grading.Score,
            grading.Feedback,
            utcNow);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new SubmitAnswerResponse(
            attempt.Id,
            question.Id,
            answer.IsCorrect,
            answer.Score,
            answer.Feedback,
            question.Explanation,
            answer.AnsweredAtUtc);
    }

    private async Task<StaticGradingResult> GradeWithAiAsync(
        Guid userId,
        string targetLanguageCode,
        string userLevel,
        string prompt,
        string userAnswer,
        string? correctAnswer,
        CancellationToken cancellationToken)
    {
        var settings = await languageSettingsReader.GetByUserIdAsync(userId, cancellationToken);
        var response = await assessmentGradingService.GradeAsync(
            new GradingRequest(
                userId,
                targetLanguageCode,
                settings?.CurrentLevel ?? userLevel,
                prompt,
                userAnswer,
                $"Grade this exercise answer from 0 to 100. Reference answer: {correctAnswer ?? "No fixed reference answer."}"),
            cancellationToken);

        var score = Math.Clamp(response.Score, 0, 100);
        return new StaticGradingResult(score >= 70, score, response.Feedback);
    }
}
