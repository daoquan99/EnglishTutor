using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using EnglishTutor.AiGateway.Domain.Aggregates.AiProvider;
using EnglishTutor.AiGateway.Domain.Aggregates.AiProvider.Repositories;
using Microsoft.EntityFrameworkCore;

namespace EnglishTutor.AiGateway.Infrastructure.Persistence.Repositories;

public class AiProviderRepository : IAiProviderRepository
{
    private readonly AiGatewayDbContext _context;

    public AiProviderRepository(AiGatewayDbContext context)
    {
        _context = context;
    }

    public async Task<AiProvider?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.Providers.FirstOrDefaultAsync(p => p.Id == id, ct);
    }

    public async Task<AiProvider?> GetByCodeAsync(string code, CancellationToken ct = default)
    {
        var codeNormalized = code.ToLowerInvariant().Trim();
        return await _context.Providers.FirstOrDefaultAsync(p => p.Code == codeNormalized, ct);
    }

    public async Task<IReadOnlyList<AiProvider>> ListAsync(CancellationToken ct = default)
    {
        return await _context.Providers
            .AsNoTracking()
            .OrderBy(p => p.Name).ThenBy(p => p.Id)
            .ToListAsync(ct);
    }

    public async Task AddAsync(AiProvider provider, CancellationToken ct = default)
    {
        await _context.Providers.AddAsync(provider, ct);
    }

    public void Update(AiProvider provider)
    {
        _context.Providers.Update(provider);
    }
}
