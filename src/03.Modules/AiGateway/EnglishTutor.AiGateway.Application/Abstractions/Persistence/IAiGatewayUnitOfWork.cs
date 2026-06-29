using System;
using System.Threading;
using System.Threading.Tasks;
using EnglishTutor.AiGateway.Domain.Aggregates.AiModel.Repositories;
using EnglishTutor.AiGateway.Domain.Aggregates.AiProvider.Repositories;
using EnglishTutor.AiGateway.Domain.Aggregates.AiProviderKey.Repositories;
using EnglishTutor.AiGateway.Domain.Aggregates.AiRouteLease.Repositories;
using EnglishTutor.AiGateway.Domain.Aggregates.AiRoutingRule.Repositories;
using EnglishTutor.AiGateway.Domain.Aggregates.AiVoice.Repositories;

namespace EnglishTutor.AiGateway.Application.Abstractions.Persistence;

/// <summary>
/// Unit of work interface for the AI Gateway module.
/// </summary>
public interface IAiGatewayUnitOfWork : IDisposable
{
    IAiProviderRepository Providers { get; }
    IAiModelRepository Models { get; }
    IAiProviderKeyRepository ProviderKeys { get; }
    IAiRoutingRuleRepository RoutingRules { get; }
    IAiRouteLeaseRepository RouteLeases { get; }
    IAiVoiceRepository Voices { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
