using EnglishTutor.BuildingBlocks.Domain.Results;

namespace EnglishTutor.Audit.Domain.SecurityEvents;

public interface ISecurityEventRepository
{
    Task AddAsync(SecurityEvent securityEvent, CancellationToken cancellationToken);
    Task<SecurityEvent?> FindByIdAsync(Guid id, CancellationToken cancellationToken);
}
