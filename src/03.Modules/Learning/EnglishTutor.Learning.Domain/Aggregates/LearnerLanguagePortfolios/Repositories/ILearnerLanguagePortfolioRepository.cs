namespace EnglishTutor.Learning.Domain.Aggregates.LearnerLanguagePortfolios.Repositories;

public interface ILearnerLanguagePortfolioRepository
{
    Task<LearnerLanguagePortfolio?> GetByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default);
    Task AddAsync(
        LearnerLanguagePortfolio portfolio,
        CancellationToken cancellationToken = default);
    void Update(LearnerLanguagePortfolio portfolio);
}
