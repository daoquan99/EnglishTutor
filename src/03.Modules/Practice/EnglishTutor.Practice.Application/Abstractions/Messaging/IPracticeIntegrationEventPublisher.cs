using System.Threading;
using System.Threading.Tasks;
using EnglishTutor.BuildingBlocks.Contracts.Events;

namespace EnglishTutor.Practice.Application.Abstractions.Messaging;

/// <summary>
/// Application abstraction to stage integration events into the Practice transactional outbox.
/// Contains no transport-specific types.
/// </summary>
public interface IPracticeIntegrationEventPublisher
{
    Task StageAsync(
        IntegrationEvent integrationEvent,
        CancellationToken cancellationToken);
}
