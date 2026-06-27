using System;
using System.Threading;
using System.Threading.Tasks;
using EnglishTutor.BuildingBlocks.Contracts.Events;
using EnglishTutor.Learning.Application.Abstractions.Messaging;
using EnglishTutor.Learning.Infrastructure.Persistence;
using MassTransit;
using MassTransit.EntityFrameworkCoreIntegration;

namespace EnglishTutor.Learning.Infrastructure.Messaging;

internal sealed class MassTransitLearningIntegrationEventPublisher : ILearningIntegrationEventPublisher
{
    private readonly IPublishEndpoint _publishEndpoint;

    public MassTransitLearningIntegrationEventPublisher(
        EntityFrameworkScopedBusContext<IBus, LearningDbContext> scopedBusContext)
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
