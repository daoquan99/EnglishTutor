using EnglishTutor.Learning.Domain.Aggregates.LanguageDefinitions.Repositories;
using EnglishTutor.Learning.Domain.Aggregates.LanguageDefinitions.ValueObjects;

namespace EnglishTutor.Learning.Application.LanguagePairs.Services;

public sealed class LanguagePairPolicy
{
    private readonly ILanguageDefinitionRepository _languages;

    public LanguagePairPolicy(ILanguageDefinitionRepository languages)
    {
        _languages = languages;
    }

    public async Task<(string Native, string Target, string Explanation)> ValidateAsync(
        string nativeLanguageCode,
        string targetLanguageCode,
        string explanationLanguageCode,
        CancellationToken cancellationToken)
    {
        var nativeCode = LanguageCode.Create(nativeLanguageCode).Value;
        var targetCode = LanguageCode.Create(targetLanguageCode).Value;
        var explanationCode = LanguageCode.Create(explanationLanguageCode).Value;

        if (nativeCode == targetCode)
        {
            throw new InvalidOperationException("Native and target languages must differ.");
        }

        var native = await _languages.GetByCodeAsync(nativeCode, cancellationToken);
        var target = await _languages.GetByCodeAsync(targetCode, cancellationToken);
        var explanation = await _languages.GetByCodeAsync(explanationCode, cancellationToken);

        if (native is not { IsActive: true, IsAvailableAsNative: true })
        {
            throw new InvalidOperationException("Native language is unavailable.");
        }

        if (target is not { IsActive: true, IsAvailableAsTarget: true })
        {
            throw new InvalidOperationException("Target language is unavailable.");
        }

        if (explanation is not { IsActive: true })
        {
            throw new InvalidOperationException("Explanation language is unavailable.");
        }

        return (nativeCode, targetCode, explanationCode);
    }
}
