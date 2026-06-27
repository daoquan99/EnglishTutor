using System;
using System.Threading;
using System.Threading.Tasks;
using EnglishTutor.BuildingBlocks.Contracts.Events;
using EnglishTutor.Feedback.Application.Abstractions.Messaging;
using EnglishTutor.Feedback.Infrastructure.Persistence;
using MassTransit;
using MassTransit.EntityFrameworkCoreIntegration;

namespace EnglishTutor.Feedback.Infrastructure.Messaging;

internal sealed class MassTransitFeedbackIntegrationEventPublisher : IFeedbackIntegrationEventPublisher
{
    private readonly IPublishEndpoint _publishEndpoint;

    public MassTransitFeedbackIntegrationEventPublisher(
        EntityFrameworkScopedBusContext<IBus, FeedbackDbContext> scopedBusContext)
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
