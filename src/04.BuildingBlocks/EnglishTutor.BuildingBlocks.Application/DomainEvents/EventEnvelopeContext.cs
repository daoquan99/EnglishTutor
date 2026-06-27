
namespace EnglishTutor.BuildingBlocks.Application.DomainEvents;

public sealed record EventEnvelopeContext(
    Guid CorrelationId,
    Guid? CausationId);
