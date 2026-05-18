namespace EnglishTutor.Modules.Progress.Application.Abstractions;

public interface IProgressUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
