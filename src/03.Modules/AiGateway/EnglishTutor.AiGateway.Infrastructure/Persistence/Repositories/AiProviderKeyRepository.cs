using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using EnglishTutor.AiGateway.Domain.Aggregates.AiProviderKey;
using EnglishTutor.AiGateway.Domain.Aggregates.AiProviderKey.Repositories;
using Microsoft.EntityFrameworkCore;

namespace EnglishTutor.AiGateway.Infrastructure.Persistence.Repositories;

public class AiProviderKeyRepository : IAiProviderKeyRepository
{
    private readonly AiGatewayDbContext _context;

    public AiProviderKeyRepository(AiGatewayDbContext context)
    {
        _context = context;
    }

    public async Task<AiProviderKey?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.ProviderKeys.FirstOrDefaultAsync(k => k.Id == id, ct);
    }

    public async Task<IReadOnlyList<AiProviderKey>> ListByProviderAsync(Guid providerId, CancellationToken ct = default)
    {
        return await _context.ProviderKeys
            .AsNoTracking()
            .Where(k => k.ProviderId == providerId)
            .OrderBy(k => k.Priority).ThenBy(k => k.Id)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<AiProviderKey>> ListActiveByProviderByPriorityAsync(Guid providerId, CancellationToken ct = default)
    {
        return await _context.ProviderKeys
            .Where(k => k.ProviderId == providerId && k.IsActive)
            .OrderBy(k => k.Priority).ThenBy(k => k.Id)
            .ToListAsync(ct);
    }

    public async Task AddAsync(AiProviderKey key, CancellationToken ct = default)
    {
        await _context.ProviderKeys.AddAsync(key, ct);
    }

    public void Update(AiProviderKey key)
    {
        _context.ProviderKeys.Update(key);
    }
}
