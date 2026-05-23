using EnglishTutor.BuildingBlocks.Domain;
using EnglishTutor.BuildingBlocks.Domain.Exceptions;
using EnglishTutor.BuildingBlocks.SharedKernel;

namespace EnglishTutor.Modules.Speaking.Domain.ValueObjects;

public sealed class LanguageSnapshot : ValueObject
{
    public LanguageCode NativeLanguageCode { get; }
    public LanguageCode TargetLanguageCode { get; }
    public LanguageCode UiLanguageCode { get; }
    public LanguageCode ExplanationLanguageCode { get; }
    public LanguageLevel UserLevel { get; }

    private LanguageSnapshot(
        LanguageCode nativeLanguageCode,
        LanguageCode targetLanguageCode,
        LanguageCode uiLanguageCode,
        LanguageCode explanationLanguageCode,
        LanguageLevel userLevel)
    {
        NativeLanguageCode = nativeLanguageCode;
        TargetLanguageCode = targetLanguageCode;
        UiLanguageCode = uiLanguageCode;
        ExplanationLanguageCode = explanationLanguageCode;
        UserLevel = userLevel;
    }

    public static LanguageSnapshot Create(
        LanguageCode nativeLanguageCode,
        LanguageCode targetLanguageCode,
        LanguageCode uiLanguageCode,
        LanguageCode explanationLanguageCode,
        LanguageLevel userLevel)
    {
        return new LanguageSnapshot(
            nativeLanguageCode ?? throw new DomainException("Native language code is required."),
            targetLanguageCode ?? throw new DomainException("Target language code is required."),
            uiLanguageCode ?? throw new DomainException("UI language code is required."),
            explanationLanguageCode ?? throw new DomainException("Explanation language code is required."),
            userLevel);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return NativeLanguageCode.Value;
        yield return TargetLanguageCode.Value;
        yield return UiLanguageCode.Value;
        yield return ExplanationLanguageCode.Value;
        yield return UserLevel;
    }
}
