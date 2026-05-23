namespace EnglishTutor.Modules.LearningContent.Application.Abstractions;

public interface ILearningContentUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
