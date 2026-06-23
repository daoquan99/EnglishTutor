using System;
using System.Threading;
using System.Threading.Tasks;

namespace EnglishTutor.Audit.Domain.Aggregates.AuditLogs.Repositories;

public interface IAuditLogRepository
{
    Task AddAsync(AuditLog auditLog, CancellationToken cancellationToken = default);
    Task<AuditLog?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
}
