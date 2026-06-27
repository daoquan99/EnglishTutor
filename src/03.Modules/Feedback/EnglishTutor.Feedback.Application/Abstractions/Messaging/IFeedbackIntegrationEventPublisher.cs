using System.Threading;
using System.Threading.Tasks;
using EnglishTutor.BuildingBlocks.Contracts.Events;

namespace EnglishTutor.Feedback.Application.Abstractions.Messaging;

/// <summary>
/// Application abstraction to stage integration events into the Feedback transactional outbox.
/// </summary>
public interface IFeedbackIntegrationEventPublisher
{
    Task StageAsync(
        IntegrationEvent integrationEvent,
        CancellationToken cancellationToken);
}
