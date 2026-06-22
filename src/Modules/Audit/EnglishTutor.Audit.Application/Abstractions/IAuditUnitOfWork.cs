namespace EnglishTutor.Audit.Application.Abstractions;

// Audit unit of work. Mirrors the Identity IIdentityUnitOfWork pattern.
// Only SaveChangesAsync is exposed at the application level.
public interface IAuditUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
