using EnglishTutor.BuildingBlocks.Application.Abstractions;
using EnglishTutor.BuildingBlocks.Application.Results;
using EnglishTutor.Modules.Exercises.Application.Abstractions;
using EnglishTutor.Modules.Exercises.Application.Shared.DTOs;
using EnglishTutor.Modules.Exercises.Application.Shared.Errors;
using EnglishTutor.Modules.Exercises.Application.Shared.Mappers;
using EnglishTutor.Modules.Exercises.Domain.ExerciseSet.Enums;
using EnglishTutor.Modules.Exercises.Domain.Shared;
using EnglishTutor.Modules.Exercises.Domain.UserExerciseAttempt.Enums;
using EnglishTutor.Modules.Exercises.Domain.UserExerciseAttempt.Entities;
using EnglishTutor.Modules.Exercises.Domain.UserExerciseAttempt.Events;

namespace EnglishTutor.Modules.Exercises.Application.Commands.CompleteExercise;

public sealed class CompleteExerciseCommandHandler(
    IExerciseAttemptRepository exerciseAttemptRepository,
    IExerciseSetRepository exerciseSetRepository,
    IExercisesUnitOfWork unitOfWork,
    IDateTimeProvider dateTimeProvider)
    : ICommandHandler<CompleteExerciseCommand, ExerciseResultResponse>
{
    public async Task<Result<ExerciseResultResponse>> Handle(CompleteExerciseCommand request, CancellationToken cancellationToken)
    {
        var attempt = await exerciseAttemptRepository.GetByIdWithAnswersAsync(request.AttemptId, cancellationToken);
        if (attempt is null)
        {
            return Result.Failure<ExerciseResultResponse>(ExerciseErrors.AttemptNotFound(request.AttemptId));
        }

        if (attempt.UserId != request.UserId)
        {
            return Result.Failure<ExerciseResultResponse>(ExerciseErrors.Forbidden);
        }

        if (attempt.Status == ExerciseAttemptStatus.Completed)
        {
            var completedSet = await exerciseSetRepository.GetByIdWithQuestionsAsync(attempt.ExerciseSetId, cancellationToken);
            return completedSet is null
                ? Result.Failure<ExerciseResultResponse>(ExerciseErrors.ExerciseSetNotFound(attempt.ExerciseSetId))
                : attempt.ToResultResponse(completedSet);
        }

        if (attempt.Answers.Count != attempt.TotalQuestions)
        {
            return Result.Failure<ExerciseResultResponse>(ExerciseErrors.NotAllQuestionsAnswered);
        }

        var exerciseSet = await exerciseSetRepository.GetByIdWithQuestionsAsync(attempt.ExerciseSetId, cancellationToken);
        if (exerciseSet is null)
        {
            return Result.Failure<ExerciseResultResponse>(ExerciseErrors.ExerciseSetNotFound(attempt.ExerciseSetId));
        }

        var questionsById = exerciseSet.Questions.ToDictionary(question => question.Id);
        var wrongAnswers = attempt.Answers
            .Where(answer => !answer.IsCorrect)
            .Select(answer =>
            {
                var question = questionsById[answer.QuestionId];
                return new ExerciseCompletedWrongAnswer(
                    question.Id,
                    question.Prompt,
                    answer.UserAnswer,
                    question.CorrectAnswer ?? string.Empty,
                    question.Explanation,
                    question.QuestionType.ToString());
            })
            .ToArray();

        var utcNow = dateTimeProvider.UtcNow;
        attempt.Complete(utcNow, exerciseSet.ExerciseType.ToString(), wrongAnswers);

        await exerciseAttemptRepository.AddResultAsync(
            UserExerciseResult.Create(
                attempt.Id,
                attempt.UserId,
                exerciseSet.Id,
                attempt.TargetLanguageCode,
                exerciseSet.ExerciseType,
                attempt.Score,
                attempt.CorrectCount,
                attempt.TotalQuestions,
                Math.Max(0, (int)(utcNow - attempt.StartedAtUtc).TotalSeconds),
                utcNow),
            cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return attempt.ToResultResponse(exerciseSet);
    }
}
