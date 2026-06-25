using System;
using System.Threading;
using System.Threading.Tasks;
using EnglishTutor.AiGateway.Domain.Aggregates.AiRouteLease;
using EnglishTutor.AiGateway.Domain.Aggregates.AiRouteLease.Repositories;
using Microsoft.EntityFrameworkCore;

namespace EnglishTutor.AiGateway.Infrastructure.Persistence.Repositories;

public class AiRouteLeaseRepository : IAiRouteLeaseRepository
{
    private readonly AiGatewayDbContext _context;

    public AiRouteLeaseRepository(AiGatewayDbContext context)
    {
        _context = context;
    }

    public async Task<AiRouteLease?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.RouteLeases.FirstOrDefaultAsync(l => l.Id == id, ct);
    }

    public async Task<AiRouteLease?> GetByIdempotencyKeyAsync(Guid userId, string idempotencyKey, CancellationToken ct = default)
    {
        var keyNormalized = idempotencyKey.Trim();
        return await _context.RouteLeases.FirstOrDefaultAsync(
            l => l.UserId == userId && l.IdempotencyKey == keyNormalized, 
            ct);
    }

    public async Task AddAsync(AiRouteLease lease, CancellationToken ct = default)
    {
        await _context.RouteLeases.AddAsync(lease, ct);
    }

    public void Update(AiRouteLease lease)
    {
        _context.RouteLeases.Update(lease);
    }
}
