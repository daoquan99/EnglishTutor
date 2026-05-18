using EnglishTutor.BuildingBlocks.Domain;
using EnglishTutor.BuildingBlocks.Domain.Exceptions;
using EnglishTutor.BuildingBlocks.SharedKernel;

namespace EnglishTutor.Modules.Mistakes.Domain.Entities;

public sealed class UserMistakeCard : Entity<Guid>
{
    public Guid UserId { get; private set; }
    public LanguageCode TargetLanguageCode { get; private set; } = default!;
    public Guid MistakeId { get; private set; }
    public DateTime DueAtUtc { get; private set; }

    private UserMistakeCard() { }

    public static UserMistakeCard Create(Guid userId, LanguageCode targetLanguageCode, Guid mistakeId, DateTime dueAtUtc)
    {
        if (userId == Guid.Empty || mistakeId == Guid.Empty)
        {
            throw new DomainException("User id and mistake id are required.");
        }

        return new UserMistakeCard
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            TargetLanguageCode = targetLanguageCode ?? throw new DomainException("Target language code is required."),
            MistakeId = mistakeId,
            DueAtUtc = dueAtUtc
        };
    }
}
