using EnglishTutor.Learning.Contracts.Dtos;

namespace EnglishTutor.Learning.Contracts;

public interface ILearningLanguageModule
{
    Task<LanguageContextDto> EnsureActiveLanguageContextAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    Task<LanguageContextDto?> GetLanguagePairAsync(
        Guid userId,
        Guid languagePairId,
        CancellationToken cancellationToken = default);
}
