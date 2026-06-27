using System;
using System.Threading;
using System.Threading.Tasks;
using EnglishTutor.BuildingBlocks.Contracts.Events;
using EnglishTutor.Practice.Application.Abstractions.Messaging;
using EnglishTutor.Practice.Infrastructure.Persistence;
using MassTransit;
using MassTransit.EntityFrameworkCoreIntegration;

namespace EnglishTutor.Practice.Infrastructure.Messaging;

internal sealed class MassTransitPracticeIntegrationEventPublisher : IPracticeIntegrationEventPublisher
{
    private readonly IPublishEndpoint _publishEndpoint;

    public MassTransitPracticeIntegrationEventPublisher(
        EntityFrameworkScopedBusContext<IBus, PracticeDbContext> scopedBusContext)
    {
        _publishEndpoint = scopedBusContext.PublishEndpoint;
    }

    public Task StageAsync(
        IntegrationEvent integrationEvent,
        CancellationToken cancellationToken)
    {
        return _publishEndpoint.Publish(
            integrationEvent,
            integrationEvent.GetType(),
            cancellationToken);
    }
}
