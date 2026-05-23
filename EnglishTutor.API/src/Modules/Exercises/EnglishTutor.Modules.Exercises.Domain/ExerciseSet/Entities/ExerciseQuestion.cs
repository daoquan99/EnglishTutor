using EnglishTutor.BuildingBlocks.Domain;
using EnglishTutor.BuildingBlocks.Domain.Exceptions;
using EnglishTutor.Modules.Exercises.Domain.ExerciseSet.Enums;
using EnglishTutor.Modules.Exercises.Domain.Shared;

namespace EnglishTutor.Modules.Exercises.Domain.ExerciseSet.Entities;

public sealed class ExerciseQuestion : Entity<Guid>
{
    private readonly List<ExerciseOption> _options = [];

    public Guid ExerciseSetId { get; private set; }
    public ExerciseType QuestionType { get; private set; }
    public string Prompt { get; private set; } = string.Empty;
    public string? CorrectAnswer { get; private set; }
    public string? Explanation { get; private set; }
    public int Order { get; private set; }
    public QuestionDifficulty Difficulty { get; private set; }
    public bool IsAiGraded { get; private set; }
    public IReadOnlyCollection<ExerciseOption> Options => _options.AsReadOnly();

    private ExerciseQuestion() { }

    internal static ExerciseQuestion Create(
        Guid exerciseSetId,
        ExerciseType questionType,
        string prompt,
        string? correctAnswer,
        string? explanation,
        int order,
        QuestionDifficulty difficulty,
        bool isAiGraded,
        DateTime utcNow)
    {
        if (exerciseSetId == Guid.Empty)
        {
            throw new DomainException("Exercise set id is required.");
        }

        return new ExerciseQuestion
        {
            Id = Guid.NewGuid(),
            ExerciseSetId = exerciseSetId,
            QuestionType = questionType,
            Prompt = global::EnglishTutor.Modules.Exercises.Domain.ExerciseSet.ExerciseSet.NormalizeRequired(prompt, 4000, "Prompt"),
            CorrectAnswer = global::EnglishTutor.Modules.Exercises.Domain.ExerciseSet.ExerciseSet.NormalizeOptional(correctAnswer, 4000, "Correct answer"),
            Explanation = global::EnglishTutor.Modules.Exercises.Domain.ExerciseSet.ExerciseSet.NormalizeOptional(explanation, 4000, "Explanation"),
            Order = order,
            Difficulty = difficulty,
            IsAiGraded = isAiGraded,
            CreatedAtUtc = utcNow
        };
    }

    public ExerciseOption AddOption(string optionText, bool isCorrect, int order, DateTime utcNow)
    {
        if (_options.Any(option => option.Order == order))
        {
            throw new DomainException("Option order must be unique within a question.");
        }

        var option = ExerciseOption.Create(Id, optionText, isCorrect, order, utcNow);
        _options.Add(option);
        return option;
    }
}
