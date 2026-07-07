using EnglishTutor.BuildingBlocks.Domain.ValueObjects;

namespace EnglishTutor.Practice.Domain.Aggregates.PracticeSession.ValueObjects;

public sealed class PracticeSessionLanguageSnapshot : ValueObject
{
    public Guid LanguagePairId { get; private set; }
    public string NativeLanguageCode { get; private set; } = string.Empty;
    public string TargetLanguageCode { get; private set; } = string.Empty;
    public string ExplanationLanguageCode { get; private set; } = string.Empty;
    public long LanguagePairVersion { get; private set; }

    private PracticeSessionLanguageSnapshot()
    {
    }

    public PracticeSessionLanguageSnapshot(
        Guid languagePairId,
        string nativeLanguageCode,
        string targetLanguageCode,
        string explanationLanguageCode,
        long languagePairVersion)
    {
        if (languagePairId == Guid.Empty)
        {
            throw new ArgumentException("Language pair ID is required.", nameof(languagePairId));
        }

        LanguagePairId = languagePairId;
        NativeLanguageCode = nativeLanguageCode;
        TargetLanguageCode = targetLanguageCode;
        ExplanationLanguageCode = explanationLanguageCode;
        LanguagePairVersion = languagePairVersion;
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return LanguagePairId;
        yield return NativeLanguageCode;
        yield return TargetLanguageCode;
        yield return ExplanationLanguageCode;
        yield return LanguagePairVersion;
    }
}
