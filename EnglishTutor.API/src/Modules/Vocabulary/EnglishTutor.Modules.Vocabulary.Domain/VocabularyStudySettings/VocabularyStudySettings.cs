using EnglishTutor.BuildingBlocks.Domain;
using EnglishTutor.BuildingBlocks.Domain.Exceptions;
using EnglishTutor.BuildingBlocks.SharedKernel;

namespace EnglishTutor.Modules.Vocabulary.Domain.Entities;

public sealed class VocabularyStudySettings : AggregateRoot<Guid>
{
    public Guid UserId { get; private set; }
    public LanguageCode TargetLanguageCode { get; private set; } = default!;
    public int NewWordsPerDay { get; private set; }
    public int ReviewWordsPerDay { get; private set; }
    public bool IncludeMasteredInReview { get; private set; }

    private VocabularyStudySettings() { }

    public static VocabularyStudySettings CreateDefault(Guid userId, LanguageCode targetLanguageCode, DateTime utcNow)
    {
        if (userId == Guid.Empty)
        {
            throw new DomainException("User id is required.");
        }

        return new VocabularyStudySettings
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            TargetLanguageCode = targetLanguageCode ?? throw new DomainException("Target language code is required."),
            NewWordsPerDay = 5,
            ReviewWordsPerDay = 20,
            IncludeMasteredInReview = false
        };
    }

    public void Update(int newWordsPerDay, int reviewWordsPerDay, bool includeMasteredInReview)
    {
        if (newWordsPerDay is < 1 or > 50)
        {
            throw new DomainException("New words per day must be between 1 and 50.");
        }

        if (reviewWordsPerDay is < 1 or > 100)
        {
            throw new DomainException("Review words per day must be between 1 and 100.");
        }

        NewWordsPerDay = newWordsPerDay;
        ReviewWordsPerDay = reviewWordsPerDay;
        IncludeMasteredInReview = includeMasteredInReview;
    }
}
