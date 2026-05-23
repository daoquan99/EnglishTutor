using EnglishTutor.BuildingBlocks.Domain;
using EnglishTutor.BuildingBlocks.Domain.Exceptions;
using EnglishTutor.BuildingBlocks.SharedKernel;
using EnglishTutor.Modules.Exercises.Domain.ExerciseSet.Enums;
using EnglishTutor.Modules.Exercises.Domain.Shared;
using EnglishTutor.Modules.Exercises.Domain.ExerciseSet.Entities;

namespace EnglishTutor.Modules.Exercises.Domain.ExerciseSet;

public sealed class ExerciseSet : AggregateRoot<Guid>
{
    private readonly List<ExerciseQuestion> _questions = [];

    public string TargetLanguageCode { get; private set; } = string.Empty;
    public LanguageLevel Level { get; private set; }
    public string Topic { get; private set; } = string.Empty;
    public LearningSkill Skill { get; private set; }
    public ExerciseType ExerciseType { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public bool IsPublished { get; private set; }
    public int TotalQuestions { get; private set; }
    public IReadOnlyCollection<ExerciseQuestion> Questions => _questions.AsReadOnly();

    private ExerciseSet() { }

    public static ExerciseSet Create(
        string targetLanguageCode,
        LanguageLevel level,
        string topic,
        LearningSkill skill,
        ExerciseType exerciseType,
        string title,
        string? description,
        DateTime utcNow)
    {
        return new ExerciseSet
        {
            Id = Guid.NewGuid(),
            TargetLanguageCode = NormalizeLanguage(targetLanguageCode),
            Level = level,
            Topic = NormalizeRequired(topic, 100, "Topic"),
            Skill = skill,
            ExerciseType = exerciseType,
            Title = NormalizeRequired(title, 200, "Title"),
            Description = NormalizeOptional(description, 1000, "Description"),
            CreatedAtUtc = utcNow
        };
    }

    public ExerciseQuestion AddQuestion(
        ExerciseType questionType,
        string prompt,
        string? correctAnswer,
        string? explanation,
        int order,
        QuestionDifficulty difficulty,
        bool isAiGraded,
        DateTime utcNow)
    {
        if (_questions.Any(question => question.Order == order))
        {
            throw new DomainException("Question order must be unique within an exercise set.");
        }

        if (!isAiGraded && string.IsNullOrWhiteSpace(correctAnswer))
        {
            throw new DomainException("Static exercise questions require a correct answer.");
        }

        var question = ExerciseQuestion.Create(
            Id,
            questionType,
            prompt,
            correctAnswer,
            explanation,
            order,
            difficulty,
            isAiGraded,
            utcNow);

        _questions.Add(question);
        TotalQuestions = _questions.Count;
        return question;
    }

    public void Publish(DateTime utcNow)
    {
        if (_questions.Count == 0)
        {
            throw new DomainException("Exercise set must have at least one question before publishing.");
        }

        IsPublished = true;
        TotalQuestions = _questions.Count;
        UpdatedAtUtc = utcNow;
    }

    internal static string NormalizeRequired(string value, int maxLength, string fieldName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new DomainException($"{fieldName} is required.");
        }

        var normalized = value.Trim();
        if (normalized.Length > maxLength)
        {
            throw new DomainException($"{fieldName} must not exceed {maxLength} characters.");
        }

        return normalized;
    }

    internal static string? NormalizeOptional(string? value, int maxLength, string fieldName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var normalized = value.Trim();
        if (normalized.Length > maxLength)
        {
            throw new DomainException($"{fieldName} must not exceed {maxLength} characters.");
        }

        return normalized;
    }

    internal static string NormalizeLanguage(string value)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Trim().Length is < 2 or > 3)
        {
            throw new DomainException("Language code must be 2-3 characters.");
        }

        return value.Trim().ToLowerInvariant();
    }
}
