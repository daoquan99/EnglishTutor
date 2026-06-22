using EnglishTutor.Audit.Application.Abstractions;

namespace EnglishTutor.Audit.Infrastructure.Persistence;

internal sealed class AuditUnitOfWork : IAuditUnitOfWork
{
    private readonly AuditDbContext _db;

    public AuditUnitOfWork(AuditDbContext db)
    {
        _db = db;
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken) =>
        _db.SaveChangesAsync(cancellationToken);
}
