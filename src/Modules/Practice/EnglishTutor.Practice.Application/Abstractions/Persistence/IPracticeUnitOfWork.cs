namespace EnglishTutor.Practice.Application.Abstractions.Persistence;

public interface IPracticeUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
