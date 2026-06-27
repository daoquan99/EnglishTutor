using System;
using System.Collections.Generic;
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

    public async Task<AiRoutingRule?> FindActiveMatchAsync(
        string activityType,
        string topicCode,
        string scenarioCode,
        CancellationToken ct = default)
    {
        var activity = activityType.ToLowerInvariant().Trim();
        var topic = topicCode.ToLowerInvariant().Trim();
        var scenario = scenarioCode.ToLowerInvariant().Trim();

        return await _context.RoutingRules
            .Where(r => r.IsActive
                && r.ActivityType == activity
                && r.TopicCode == topic
                && r.ScenarioCode == scenario)
            .OrderBy(r => r.Id)
            .FirstOrDefaultAsync(ct);
    }

    public async Task<IReadOnlyList<AiRoutingRule>> ListAsync(CancellationToken ct = default)
    {
        return await _context.RoutingRules
            .AsNoTracking()
            .OrderBy(r => r.Name).ThenBy(r => r.Id)
            .ToListAsync(ct);
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
