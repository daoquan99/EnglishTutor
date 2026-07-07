using EnglishTutor.BuildingBlocks.Domain.Aggregates;
using EnglishTutor.Learning.Domain.Aggregates.LearnerLanguagePortfolios.Entities;
using EnglishTutor.Learning.Domain.Aggregates.LearnerLanguagePortfolios.Events;

namespace EnglishTutor.Learning.Domain.Aggregates.LearnerLanguagePortfolios;

public sealed class LearnerLanguagePortfolio : AggregateRoot
{
    private readonly List<LearnerLanguagePair> _languagePairs = [];

    public Guid UserId { get; private set; }
    public Guid? ActiveLanguagePairId { get; private set; }
    public long Version { get; private set; }
    public IReadOnlyCollection<LearnerLanguagePair> LanguagePairs => _languagePairs.AsReadOnly();

    private LearnerLanguagePortfolio()
    {
    }

    public static LearnerLanguagePortfolio Create(Guid id, Guid userId)
    {
        if (userId == Guid.Empty)
        {
            throw new ArgumentException("User ID is required.", nameof(userId));
        }

        return new LearnerLanguagePortfolio
        {
            Id = id,
            UserId = userId,
            Version = 1
        };
    }

    public LearnerLanguagePair AddPair(
        Guid pairId,
        string nativeLanguageCode,
        string targetLanguageCode,
        string explanationLanguageCode)
    {
        if (string.Equals(nativeLanguageCode, targetLanguageCode, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Native and target languages must differ.");
        }

        if (_languagePairs.Any(pair =>
                pair.Status != LearnerLanguagePairStatus.Archived &&
                string.Equals(pair.NativeLanguageCode, nativeLanguageCode, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(pair.TargetLanguageCode, targetLanguageCode, StringComparison.OrdinalIgnoreCase)))
        {
            throw new InvalidOperationException("The language pair already exists.");
        }

        var status = ActiveLanguagePairId is null
            ? LearnerLanguagePairStatus.Active
            : LearnerLanguagePairStatus.Inactive;
        var pair = LearnerLanguagePair.Create(
            pairId,
            Id,
            nativeLanguageCode,
            targetLanguageCode,
            explanationLanguageCode,
            status);

        _languagePairs.Add(pair);
        ActiveLanguagePairId ??= pair.Id;
        Version++;
        RaiseDomainEvent(new LearnerLanguagePairAddedDomainEvent(Id, UserId, pair.Id));
        return pair;
    }

    public void ActivatePair(Guid pairId)
    {
        var pair = GetPair(pairId);
        if (pair.Status == LearnerLanguagePairStatus.Archived)
        {
            throw new InvalidOperationException("An archived language pair cannot be activated.");
        }

        if (ActiveLanguagePairId == pairId)
        {
            return;
        }

        foreach (var current in _languagePairs.Where(item =>
                     item.Status == LearnerLanguagePairStatus.Active))
        {
            current.Deactivate();
        }

        pair.Activate();
        ActiveLanguagePairId = pair.Id;
        Version++;
        RaiseDomainEvent(new LearnerActiveLanguagePairChangedDomainEvent(Id, UserId, pair.Id));
    }

    public void UpdateExplanationLanguage(Guid pairId, string explanationLanguageCode)
    {
        var pair = GetPair(pairId);
        if (pair.Status == LearnerLanguagePairStatus.Archived)
        {
            throw new InvalidOperationException("An archived language pair cannot be updated.");
        }

        pair.UpdateExplanationLanguage(explanationLanguageCode);
        Version++;
    }

    public void ArchivePair(Guid pairId)
    {
        var pair = GetPair(pairId);
        if (ActiveLanguagePairId == pairId)
        {
            throw new InvalidOperationException("Activate another language pair before archiving the active pair.");
        }

        if (pair.Status == LearnerLanguagePairStatus.Archived)
        {
            return;
        }

        pair.Archive();
        Version++;
        RaiseDomainEvent(new LearnerLanguagePairArchivedDomainEvent(Id, UserId, pair.Id));
    }

    public LearnerLanguagePair GetActivePair()
    {
        if (ActiveLanguagePairId is not Guid pairId)
        {
            throw new InvalidOperationException("The portfolio has no active language pair.");
        }

        return GetPair(pairId);
    }

    private LearnerLanguagePair GetPair(Guid pairId) =>
        _languagePairs.FirstOrDefault(pair => pair.Id == pairId)
        ?? throw new KeyNotFoundException("Language pair was not found.");
}
