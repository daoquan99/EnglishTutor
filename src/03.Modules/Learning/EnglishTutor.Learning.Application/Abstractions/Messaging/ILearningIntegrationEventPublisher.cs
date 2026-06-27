using System.Threading;
using System.Threading.Tasks;
using EnglishTutor.BuildingBlocks.Contracts.Events;

namespace EnglishTutor.Learning.Application.Abstractions.Messaging;

/// <summary>
/// Application abstraction to stage integration events into the Learning transactional outbox.
/// </summary>
public interface ILearningIntegrationEventPublisher
{
    Task StageAsync(
        IntegrationEvent integrationEvent,
        CancellationToken cancellationToken);
}
