using EnglishTutor.Modules.Exercises.Application.Queries.GetExerciseById;
using EnglishTutor.Modules.Exercises.Application.Queries.GetExercises;
using EnglishTutor.Modules.Exercises.Application.Shared.DTOs;
using EnglishTutor.Modules.Exercises.Domain.ExerciseSet;
using EnglishTutor.Modules.Exercises.Domain.UserExerciseAttempt;

namespace EnglishTutor.Modules.Exercises.Application.Shared.Mappers;

internal static class ExerciseMappers
{
    public static ExerciseListResponse ToListResponse(this ExerciseSet exerciseSet) =>
        new(
            exerciseSet.Id,
            exerciseSet.TargetLanguageCode,
            exerciseSet.Level.ToString(),
            exerciseSet.Topic,
            exerciseSet.Skill.ToString(),
            exerciseSet.ExerciseType.ToString(),
            exerciseSet.Title,
            exerciseSet.Description,
            exerciseSet.TotalQuestions);

    public static ExerciseDetailResponse ToDetailResponse(this ExerciseSet exerciseSet) =>
        new(
            exerciseSet.Id,
            exerciseSet.TargetLanguageCode,
            exerciseSet.Level.ToString(),
            exerciseSet.Topic,
            exerciseSet.Skill.ToString(),
            exerciseSet.ExerciseType.ToString(),
            exerciseSet.Title,
            exerciseSet.Description,
            exerciseSet.TotalQuestions,
            exerciseSet.Questions
                .OrderBy(question => question.Order)
                .Select(question => new ExerciseQuestionResponse(
                    question.Id,
                    question.QuestionType.ToString(),
                    question.Prompt,
                    null,
                    question.Order,
                    question.Difficulty.ToString(),
                    question.IsAiGraded,
                    question.Options
                        .OrderBy(option => option.Order)
                        .Select(option => new ExerciseOptionResponse(option.Id, option.OptionText, option.Order))
                        .ToArray()))
                .ToArray());

    public static ExerciseResultResponse ToResultResponse(this UserExerciseAttempt attempt, ExerciseSet exerciseSet)
    {
        var questionsById = exerciseSet.Questions.ToDictionary(question => question.Id);
        var completedAtUtc = attempt.CompletedAtUtc ?? attempt.StartedAtUtc;

        return new ExerciseResultResponse(
            attempt.Id,
            exerciseSet.Id,
            exerciseSet.ExerciseType.ToString(),
            attempt.TargetLanguageCode,
            attempt.Score,
            attempt.CorrectCount,
            attempt.TotalQuestions,
            Math.Max(0, (int)(completedAtUtc - attempt.StartedAtUtc).TotalSeconds),
            attempt.StartedAtUtc,
            completedAtUtc,
            attempt.Answers
                .OrderBy(answer => questionsById.TryGetValue(answer.QuestionId, out var question) ? question.Order : int.MaxValue)
                .Select(answer =>
                {
                    questionsById.TryGetValue(answer.QuestionId, out var question);
                    return new ExerciseAnswerResultResponse(
                        answer.QuestionId,
                        question?.QuestionType.ToString() ?? string.Empty,
                        question?.Prompt ?? string.Empty,
                        answer.UserAnswer,
                        question?.CorrectAnswer,
                        answer.IsCorrect,
                        answer.Score,
                        answer.Feedback,
                        question?.Explanation,
                        answer.AnsweredAtUtc);
                })
                .ToArray());
    }
}
