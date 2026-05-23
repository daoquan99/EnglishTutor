using EnglishTutor.BuildingBlocks.Domain;
using EnglishTutor.BuildingBlocks.Domain.Exceptions;

namespace EnglishTutor.Modules.Exercises.Domain.ExerciseSet.Entities;

public sealed class ExerciseOption : Entity<Guid>
{
    public Guid QuestionId { get; private set; }
    public string OptionText { get; private set; } = string.Empty;
    public bool IsCorrect { get; private set; }
    public int Order { get; private set; }

    private ExerciseOption() { }

    internal static ExerciseOption Create(Guid questionId, string optionText, bool isCorrect, int order, DateTime utcNow)
    {
        if (questionId == Guid.Empty)
        {
            throw new DomainException("Question id is required.");
        }

        return new ExerciseOption
        {
            Id = Guid.NewGuid(),
            QuestionId = questionId,
            OptionText = global::EnglishTutor.Modules.Exercises.Domain.ExerciseSet.ExerciseSet.NormalizeRequired(optionText, 1000, "Option text"),
            IsCorrect = isCorrect,
            Order = order,
            CreatedAtUtc = utcNow
        };
    }
}
