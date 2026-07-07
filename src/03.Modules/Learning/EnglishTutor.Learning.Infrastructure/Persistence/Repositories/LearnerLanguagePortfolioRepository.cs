using EnglishTutor.Learning.Domain.Aggregates.LearnerLanguagePortfolios;
using EnglishTutor.Learning.Domain.Aggregates.LearnerLanguagePortfolios.Repositories;
using Microsoft.EntityFrameworkCore;

namespace EnglishTutor.Learning.Infrastructure.Persistence.Repositories;

internal sealed class LearnerLanguagePortfolioRepository
    : ILearnerLanguagePortfolioRepository
{
    private readonly LearningDbContext _dbContext;

    public LearnerLanguagePortfolioRepository(LearningDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<LearnerLanguagePortfolio?> GetByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default) =>
        _dbContext.LearnerLanguagePortfolios
            .Include(item => item.LanguagePairs)
            .FirstOrDefaultAsync(item => item.UserId == userId, cancellationToken);

    public Task AddAsync(
        LearnerLanguagePortfolio portfolio,
        CancellationToken cancellationToken = default) =>
        _dbContext.LearnerLanguagePortfolios.AddAsync(portfolio, cancellationToken).AsTask();

    public void Update(LearnerLanguagePortfolio portfolio) =>
        _dbContext.LearnerLanguagePortfolios.Update(portfolio);
}
