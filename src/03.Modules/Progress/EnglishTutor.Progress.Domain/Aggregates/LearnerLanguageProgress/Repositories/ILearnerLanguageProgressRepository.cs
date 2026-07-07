namespace EnglishTutor.Progress.Domain.Aggregates.LearnerLanguageProgress.Repositories;

public interface ILearnerLanguageProgressRepository
{
    Task<LearnerLanguageProgress?> GetAsync(
        Guid userId,
        Guid languagePairId,
        CancellationToken cancellationToken = default);
    Task<IReadOnlyList<LearnerLanguageProgress>> ListByUserAsync(
        Guid userId,
        CancellationToken cancellationToken = default);
    Task AddAsync(
        LearnerLanguageProgress progress,
        CancellationToken cancellationToken = default);
    void Update(LearnerLanguageProgress progress);
}
