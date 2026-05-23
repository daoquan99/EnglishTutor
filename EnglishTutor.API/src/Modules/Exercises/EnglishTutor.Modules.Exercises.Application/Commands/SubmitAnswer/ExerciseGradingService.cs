using EnglishTutor.Modules.Exercises.Domain.ExerciseSet.Enums;
using EnglishTutor.Modules.Exercises.Domain.Shared;
using EnglishTutor.Modules.Exercises.Domain.ExerciseSet.Entities;

namespace EnglishTutor.Modules.Exercises.Application.Commands.SubmitAnswer;

public sealed class ExerciseGradingService
{
    public StaticGradingResult GradeStaticAnswer(ExerciseQuestion question, string userAnswer)
    {
        if (question.QuestionType == ExerciseType.MultipleChoice)
        {
            return GradeMultipleChoice(question, userAnswer);
        }

        return GradeStaticAnswer(question.QuestionType, userAnswer, question.CorrectAnswer ?? string.Empty);
    }

    public StaticGradingResult GradeStaticAnswer(ExerciseType type, string userAnswer, string correctAnswer)
    {
        var normalizedUserAnswer = Normalize(userAnswer);
        var normalizedCorrectAnswer = Normalize(correctAnswer);
        var isCorrect = type switch
        {
            ExerciseType.FillInTheBlank or ExerciseType.VerbConjugation => normalizedUserAnswer == normalizedCorrectAnswer,
            ExerciseType.SentenceCorrection or ExerciseType.SentenceOrdering => NormalizeSentence(userAnswer) == NormalizeSentence(correctAnswer),
            _ => normalizedUserAnswer == normalizedCorrectAnswer
        };

        return new StaticGradingResult(isCorrect, isCorrect ? 100 : 0, isCorrect ? "Correct." : "Incorrect.");
    }

    private static StaticGradingResult GradeMultipleChoice(ExerciseQuestion question, string userAnswer)
    {
        var selected = Guid.TryParse(userAnswer, out var optionId)
            ? question.Options.FirstOrDefault(option => option.Id == optionId)
            : question.Options.FirstOrDefault(option => Normalize(option.OptionText) == Normalize(userAnswer));

        var isCorrect = selected?.IsCorrect == true;
        return new StaticGradingResult(isCorrect, isCorrect ? 100 : 0, isCorrect ? "Correct." : "Incorrect.");
    }

    private static string Normalize(string value) =>
        string.Join(' ', value.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries)).ToLowerInvariant();

    private static string NormalizeSentence(string value)
    {
        var chars = value
            .Where(character => char.IsLetterOrDigit(character) || char.IsWhiteSpace(character))
            .ToArray();

        return Normalize(new string(chars));
    }
}

public sealed record StaticGradingResult(bool IsCorrect, int Score, string Feedback);
