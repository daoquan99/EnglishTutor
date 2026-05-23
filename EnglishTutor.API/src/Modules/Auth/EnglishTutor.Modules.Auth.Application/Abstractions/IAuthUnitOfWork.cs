namespace EnglishTutor.Modules.Auth.Application.Abstractions;

public interface IAuthUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
