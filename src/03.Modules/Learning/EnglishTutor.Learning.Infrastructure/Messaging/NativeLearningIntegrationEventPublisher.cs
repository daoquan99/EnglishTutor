using EnglishTutor.BuildingBlocks.Contracts.Events;
using EnglishTutor.BuildingBlocks.Infrastructure.Messaging;
using EnglishTutor.Learning.Application.Abstractions.Messaging;
using EnglishTutor.Learning.Infrastructure.Persistence;

namespace EnglishTutor.Learning.Infrastructure.Messaging;

internal sealed class NativeLearningIntegrationEventPublisher : ILearningIntegrationEventPublisher
{
    private readonly NativeOutboxWriter<LearningDbContext> _outbox;

    public NativeLearningIntegrationEventPublisher(NativeOutboxWriter<LearningDbContext> outbox)
    {
        _outbox = outbox;
    }

    public Task StageAsync(
        IntegrationEvent integrationEvent,
        CancellationToken cancellationToken) =>
        _outbox.StageAsync(integrationEvent, cancellationToken);
}
