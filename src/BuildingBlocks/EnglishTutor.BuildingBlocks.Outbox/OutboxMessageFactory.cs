using EnglishTutor.BuildingBlocks.EventBus;

namespace EnglishTutor.BuildingBlocks.Outbox;

public static class OutboxMessageFactory
{
    public static OutboxMessage Create(IntegrationEvent integrationEvent, string sourceModule, string payload) =>
        new()
        {
            EventId = integrationEvent.EventId,
            EventType = ResolveStableTypeName(integrationEvent.GetType()),
            Payload = payload,
            SourceModule = sourceModule,
            CreatedAtUtc = integrationEvent.OccurredOnUtc
        };

    // Avoid AssemblyQualifiedName: the version/culture/pubkey-token fields cause
    // Type.GetType to fail across deploys when the assembly version changes.
    public static string ResolveStableTypeName(Type type) =>
        $"{type.FullName}, {type.Assembly.GetName().Name}";
}
