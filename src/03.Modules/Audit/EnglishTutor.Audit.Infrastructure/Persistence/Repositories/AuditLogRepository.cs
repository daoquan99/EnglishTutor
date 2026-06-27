using System;
using System.Threading;
using System.Threading.Tasks;
using EnglishTutor.Audit.Domain.Aggregates.AuditLogs;
using EnglishTutor.Audit.Domain.Aggregates.AuditLogs.Repositories;
using Microsoft.EntityFrameworkCore;

namespace EnglishTutor.Audit.Infrastructure.Persistence.Repositories;

internal sealed class AuditLogRepository : IAuditLogRepository
{
    private readonly AuditDbContext _db;

    public AuditLogRepository(AuditDbContext db)
    {
        _db = db;
    }

    public async Task AddAsync(AuditLog auditLog, CancellationToken cancellationToken = default)
    {
        await _db.AuditLogs.AddAsync(auditLog, cancellationToken);
    }

    public Task<AuditLog?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return _db.AuditLogs.FirstOrDefaultAsync(l => l.Id == id, cancellationToken);
    }
}
