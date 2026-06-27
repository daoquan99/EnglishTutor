using EnglishTutor.Audit.Domain.Aggregates.SecurityEvents;
using EnglishTutor.Audit.Domain.Aggregates.SecurityEvents.Repositories;
using Microsoft.EntityFrameworkCore;

namespace EnglishTutor.Audit.Infrastructure.Persistence.Repositories;

internal sealed class SecurityEventRepository : ISecurityEventRepository
{
    private readonly AuditDbContext _db;

    public SecurityEventRepository(AuditDbContext db)
    {
        _db = db;
    }

    public async Task AddAsync(SecurityEvent securityEvent, CancellationToken cancellationToken)
    {
        await _db.SecurityEvents.AddAsync(securityEvent, cancellationToken);
    }

    public Task<SecurityEvent?> FindByIdAsync(Guid id, CancellationToken cancellationToken) =>
        _db.SecurityEvents.FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
}
