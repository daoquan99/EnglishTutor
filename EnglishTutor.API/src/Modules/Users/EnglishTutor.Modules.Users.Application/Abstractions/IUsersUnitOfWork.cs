namespace EnglishTutor.Modules.Users.Application.Abstractions;

public interface IUsersUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
