using System;
using System.Threading;
using System.Threading.Tasks;
using EnglishTutor.AiGateway.Domain.Aggregates.AiRoutingRule;
using EnglishTutor.AiGateway.Domain.Aggregates.AiRoutingRule.Repositories;
using Microsoft.EntityFrameworkCore;

namespace EnglishTutor.AiGateway.Infrastructure.Persistence.Repositories;

public class AiRoutingRuleRepository : IAiRoutingRuleRepository
{
    private readonly AiGatewayDbContext _context;

    public AiRoutingRuleRepository(AiGatewayDbContext context)
    {
        _context = context;
    }

    public async Task<AiRoutingRule?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.RoutingRules.FirstOrDefaultAsync(r => r.Id == id, ct);
    }

    public async Task AddAsync(AiRoutingRule rule, CancellationToken ct = default)
    {
        await _context.RoutingRules.AddAsync(rule, ct);
    }

    public void Update(AiRoutingRule rule)
    {
        _context.RoutingRules.Update(rule);
    }
}
