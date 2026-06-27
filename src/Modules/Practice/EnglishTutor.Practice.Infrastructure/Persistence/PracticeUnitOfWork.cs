using System.Threading;
using System.Threading.Tasks;
using EnglishTutor.Practice.Application.Abstractions.Persistence;

namespace EnglishTutor.Practice.Infrastructure.Persistence;

internal sealed class PracticeUnitOfWork : IPracticeUnitOfWork
{
    private readonly PracticeDbContext _db;

    public PracticeUnitOfWork(PracticeDbContext db)
    {
        _db = db;
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        _db.SaveChangesAsync(cancellationToken);
}
