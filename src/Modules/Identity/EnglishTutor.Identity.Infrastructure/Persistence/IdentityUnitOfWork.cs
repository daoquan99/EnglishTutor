using EnglishTutor.Identity.Application.Abstractions.Persistence;

namespace EnglishTutor.Identity.Infrastructure.Persistence;

/// <summary>
/// Infrastructure implementation of <see cref="IIdentityUnitOfWork"/>.
/// Wraps the <c>IdentityDbContext.SaveChangesAsync</c> call. Registered as
/// scoped so it shares the same <c>IdentityDbContext</c> instance as the
/// request handlers.
/// </summary>
internal sealed class IdentityUnitOfWork : IIdentityUnitOfWork
{
    private readonly IdentityDbContext _db;

    public IdentityUnitOfWork(IdentityDbContext db)
    {
        _db = db;
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken) =>
        _db.SaveChangesAsync(cancellationToken);
}
