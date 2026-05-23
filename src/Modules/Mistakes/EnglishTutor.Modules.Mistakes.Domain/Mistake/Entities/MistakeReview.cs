using EnglishTutor.BuildingBlocks.Domain;
using EnglishTutor.BuildingBlocks.Domain.Exceptions;

namespace EnglishTutor.Modules.Mistakes.Domain.Entities;

public sealed class MistakeReview : Entity<Guid>
{
    public Guid MistakeId { get; private set; }
    public Guid UserId { get; private set; }
    public bool WasCorrect { get; private set; }
    public string AnswerText { get; private set; } = string.Empty;
    public DateTime ReviewedAtUtc { get; private set; }

    private MistakeReview() { }

    public static MistakeReview Create(Guid mistakeId, Guid userId, bool wasCorrect, string answerText, DateTime utcNow)
    {
        if (mistakeId == Guid.Empty || userId == Guid.Empty)
        {
            throw new DomainException("Mistake id and user id are required.");
        }

        return new MistakeReview
        {
            Id = Guid.NewGuid(),
            MistakeId = mistakeId,
            UserId = userId,
            WasCorrect = wasCorrect,
            AnswerText = Mistake.Normalize(answerText, 4000, "Answer text"),
            ReviewedAtUtc = utcNow
        };
    }
}
