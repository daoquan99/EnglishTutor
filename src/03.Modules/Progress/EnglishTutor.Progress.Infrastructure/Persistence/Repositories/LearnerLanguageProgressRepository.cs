using EnglishTutor.Progress.Domain.Aggregates.LearnerLanguageProgress;
using EnglishTutor.Progress.Domain.Aggregates.LearnerLanguageProgress.Repositories;
using Microsoft.EntityFrameworkCore;

namespace EnglishTutor.Progress.Infrastructure.Persistence.Repositories;

internal sealed class LearnerLanguageProgressRepository
    : ILearnerLanguageProgressRepository
{
    private readonly ProgressDbContext _dbContext;

    public LearnerLanguageProgressRepository(ProgressDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<LearnerLanguageProgress?> GetAsync(
        Guid userId,
        Guid languagePairId,
        CancellationToken cancellationToken = default) =>
        _dbContext.LearnerLanguageProgress.FirstOrDefaultAsync(
            item => item.UserId == userId && item.LanguagePairId == languagePairId,
            cancellationToken);

    public async Task<IReadOnlyList<LearnerLanguageProgress>> ListByUserAsync(
        Guid userId,
        CancellationToken cancellationToken = default) =>
        await _dbContext.LearnerLanguageProgress
            .AsNoTracking()
            .Where(item => item.UserId == userId)
            .OrderByDescending(item => item.LastPracticedAtUtc)
            .ThenBy(item => item.LanguagePairId)
            .ToListAsync(cancellationToken);

    public Task AddAsync(
        LearnerLanguageProgress progress,
        CancellationToken cancellationToken = default) =>
        _dbContext.LearnerLanguageProgress.AddAsync(progress, cancellationToken).AsTask();

    public void Update(LearnerLanguageProgress progress) =>
        _dbContext.LearnerLanguageProgress.Update(progress);
}
