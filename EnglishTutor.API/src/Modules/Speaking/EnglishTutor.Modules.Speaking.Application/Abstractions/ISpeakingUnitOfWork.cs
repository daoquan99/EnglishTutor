namespace EnglishTutor.Modules.Speaking.Application.Abstractions;

public interface ISpeakingUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
