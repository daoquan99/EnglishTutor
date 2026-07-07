using EnglishTutor.Learning.Application.Abstractions.Persistence;
using EnglishTutor.Learning.Contracts;
using EnglishTutor.Learning.Contracts.Dtos;
using EnglishTutor.Learning.Domain.Aggregates.LanguageDefinitions.Repositories;
using EnglishTutor.Learning.Domain.Aggregates.LanguageDefinitions.ValueObjects;
using EnglishTutor.Learning.Domain.Aggregates.LearnerLanguagePortfolios;
using EnglishTutor.Learning.Domain.Aggregates.LearnerLanguagePortfolios.Repositories;

namespace EnglishTutor.Learning.Application.LanguagePairs.Services;

public sealed class LearningLanguageModuleService : ILearningLanguageModule
{
    private const string DefaultNativeLanguageCode = "vi";
    private const string DefaultTargetLanguageCode = "en";

    private readonly ILearnerLanguagePortfolioRepository _portfolios;
    private readonly ILanguageDefinitionRepository _languages;
    private readonly ILearningUnitOfWork _unitOfWork;

    public LearningLanguageModuleService(
        ILearnerLanguagePortfolioRepository portfolios,
        ILanguageDefinitionRepository languages,
        ILearningUnitOfWork unitOfWork)
    {
        _portfolios = portfolios;
        _languages = languages;
        _unitOfWork = unitOfWork;
    }

    public async Task<LanguageContextDto> EnsureActiveLanguageContextAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var portfolio = await _portfolios.GetByUserIdAsync(userId, cancellationToken);
        if (portfolio is null)
        {
            await ValidatePairAsync(
                DefaultNativeLanguageCode,
                DefaultTargetLanguageCode,
                DefaultNativeLanguageCode,
                cancellationToken);

            portfolio = LearnerLanguagePortfolio.Create(Guid.NewGuid(), userId);
            portfolio.AddPair(
                Guid.NewGuid(),
                DefaultNativeLanguageCode,
                DefaultTargetLanguageCode,
                DefaultNativeLanguageCode);
            await _portfolios.AddAsync(portfolio, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        return Map(portfolio.GetActivePair());
    }

    public async Task<LanguageContextDto?> GetLanguagePairAsync(
        Guid userId,
        Guid languagePairId,
        CancellationToken cancellationToken = default)
    {
        var portfolio = await _portfolios.GetByUserIdAsync(userId, cancellationToken);
        var pair = portfolio?.LanguagePairs.FirstOrDefault(item => item.Id == languagePairId);
        return pair is null ? null : Map(pair);
    }

    public async Task ValidatePairAsync(
        string nativeLanguageCode,
        string targetLanguageCode,
        string explanationLanguageCode,
        CancellationToken cancellationToken)
    {
        var nativeCode = LanguageCode.Create(nativeLanguageCode).Value;
        var targetCode = LanguageCode.Create(targetLanguageCode).Value;
        var explanationCode = LanguageCode.Create(explanationLanguageCode).Value;

        if (string.Equals(nativeCode, targetCode, StringComparison.OrdinalIgnoreCase))
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
    }

    private static LanguageContextDto Map(
        Domain.Aggregates.LearnerLanguagePortfolios.Entities.LearnerLanguagePair pair) =>
        new(
            LanguagePairId: pair.Id,
            NativeLanguageCode: pair.NativeLanguageCode,
            TargetLanguageCode: pair.TargetLanguageCode,
            ExplanationLanguageCode: pair.ExplanationLanguageCode,
            Version: pair.Version);
}
