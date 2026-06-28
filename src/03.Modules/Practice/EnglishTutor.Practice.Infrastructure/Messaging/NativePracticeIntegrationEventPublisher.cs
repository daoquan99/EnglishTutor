using EnglishTutor.BuildingBlocks.Contracts.Events;
using EnglishTutor.BuildingBlocks.Infrastructure.Messaging;
using EnglishTutor.Practice.Application.Abstractions.Messaging;
using EnglishTutor.Practice.Infrastructure.Persistence;

namespace EnglishTutor.Practice.Infrastructure.Messaging;

internal sealed class NativePracticeIntegrationEventPublisher : IPracticeIntegrationEventPublisher
{
    private readonly NativeOutboxWriter<PracticeDbContext> _outbox;

    public NativePracticeIntegrationEventPublisher(NativeOutboxWriter<PracticeDbContext> outbox)
    {
        _outbox = outbox;
    }

    public Task StageAsync(
        IntegrationEvent integrationEvent,
        CancellationToken cancellationToken) =>
        _outbox.StageAsync(integrationEvent, cancellationToken);
}
