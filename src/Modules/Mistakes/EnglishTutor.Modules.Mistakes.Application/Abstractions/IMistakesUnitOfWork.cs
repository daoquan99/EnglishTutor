namespace EnglishTutor.Modules.Mistakes.Application.Abstractions;

public interface IMistakesUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
