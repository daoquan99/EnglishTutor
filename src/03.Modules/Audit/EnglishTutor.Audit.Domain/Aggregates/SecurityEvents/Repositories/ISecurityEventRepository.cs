using EnglishTutor.BuildingBlocks.Domain.Results;
using EnglishTutor.Audit.Domain.Aggregates.SecurityEvents;

namespace EnglishTutor.Audit.Domain.Aggregates.SecurityEvents.Repositories;

public interface ISecurityEventRepository
{
    Task AddAsync(SecurityEvent securityEvent, CancellationToken cancellationToken);
    Task<SecurityEvent?> FindByIdAsync(Guid id, CancellationToken cancellationToken);
}
