using System;
using System.Threading;
using System.Threading.Tasks;
using EnglishTutor.AiGateway.Application.Abstractions.Persistence;
using EnglishTutor.AiGateway.Domain.Aggregates.AiModel.Repositories;
using EnglishTutor.AiGateway.Domain.Aggregates.AiProvider.Repositories;
using EnglishTutor.AiGateway.Domain.Aggregates.AiProviderKey.Repositories;
using EnglishTutor.AiGateway.Domain.Aggregates.AiRouteLease.Repositories;
using EnglishTutor.AiGateway.Domain.Aggregates.AiRoutingRule.Repositories;
using EnglishTutor.AiGateway.Infrastructure.Persistence.Repositories;

namespace EnglishTutor.AiGateway.Infrastructure.Persistence;

/// <summary>
/// Entity Framework Core implementation of the IAiGatewayUnitOfWork.
/// </summary>
public class AiGatewayUnitOfWork : IAiGatewayUnitOfWork
{
    private readonly AiGatewayDbContext _context;
    private IAiProviderRepository? _providers;
    private IAiModelRepository? _models;
    private IAiProviderKeyRepository? _providerKeys;
    private IAiRoutingRuleRepository? _routingRules;
    private IAiRouteLeaseRepository? _routeLeases;

    public AiGatewayUnitOfWork(AiGatewayDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public IAiProviderRepository Providers => _providers ??= new AiProviderRepository(_context);

    public IAiModelRepository Models => _models ??= new AiModelRepository(_context);

    public IAiProviderKeyRepository ProviderKeys => _providerKeys ??= new AiProviderKeyRepository(_context);

    public IAiRoutingRuleRepository RoutingRules => _routingRules ??= new AiRoutingRuleRepository(_context);

    public IAiRouteLeaseRepository RouteLeases => _routeLeases ??= new AiRouteLeaseRepository(_context);

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
