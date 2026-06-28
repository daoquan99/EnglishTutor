using System.Text.Json;
using EnglishTutor.BuildingBlocks.Contracts.Events;
using EnglishTutor.BuildingBlocks.Infrastructure.Outbox;

namespace EnglishTutor.BuildingBlocks.Infrastructure.Messaging;

public sealed class NativeOutboxWriter<TDbContext>
    where TDbContext : IOutboxDbContext
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);
    private readonly TDbContext _dbContext;
    private readonly IMessageContractRegistry _registry;

    public NativeOutboxWriter(TDbContext dbContext, IMessageContractRegistry registry)
    {
        _dbContext = dbContext;
        _registry = registry;
    }

    public Task StageAsync(IntegrationEvent integrationEvent, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var descriptor = _registry.Get(integrationEvent.GetType());
        var payload = JsonSerializer.Serialize(
            integrationEvent,
            integrationEvent.GetType(),
            SerializerOptions);
        _dbContext.OutboxMessages.Add(OutboxMessage.Create(
            integrationEvent: integrationEvent,
            descriptor: descriptor,
            payloadJson: payload,
            createdAtUtc: DateTimeOffset.UtcNow));
        return Task.CompletedTask;
    }
}
