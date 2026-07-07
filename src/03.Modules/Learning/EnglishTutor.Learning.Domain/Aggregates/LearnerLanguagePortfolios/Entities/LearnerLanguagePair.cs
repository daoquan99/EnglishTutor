using EnglishTutor.BuildingBlocks.Domain.Entities;

namespace EnglishTutor.Learning.Domain.Aggregates.LearnerLanguagePortfolios.Entities;

public sealed class LearnerLanguagePair : Entity
{
    public Guid PortfolioId { get; private set; }
    public string NativeLanguageCode { get; private set; } = string.Empty;
    public string TargetLanguageCode { get; private set; } = string.Empty;
    public string ExplanationLanguageCode { get; private set; } = string.Empty;
    public LearnerLanguagePairStatus Status { get; private set; }
    public long Version { get; private set; }

    private LearnerLanguagePair()
    {
    }

    internal static LearnerLanguagePair Create(
        Guid id,
        Guid portfolioId,
        string nativeLanguageCode,
        string targetLanguageCode,
        string explanationLanguageCode,
        LearnerLanguagePairStatus status)
    {
        return new LearnerLanguagePair
        {
            Id = id,
            PortfolioId = portfolioId,
            NativeLanguageCode = nativeLanguageCode,
            TargetLanguageCode = targetLanguageCode,
            ExplanationLanguageCode = explanationLanguageCode,
            Status = status,
            Version = 1
        };
    }

    internal void Activate()
    {
        Status = LearnerLanguagePairStatus.Active;
        Version++;
    }

    internal void Deactivate()
    {
        Status = LearnerLanguagePairStatus.Inactive;
        Version++;
    }

    internal void UpdateExplanationLanguage(string explanationLanguageCode)
    {
        ExplanationLanguageCode = explanationLanguageCode;
        Version++;
    }

    internal void Archive()
    {
        Status = LearnerLanguagePairStatus.Archived;
        Version++;
    }
}
