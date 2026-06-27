namespace EnglishTutor.BuildingBlocks.Application.DomainEvents;

/// <summary>
/// Allows infrastructure boundaries to set the current host-neutral event envelope context.
/// </summary>
public interface IEventEnvelopeContextSetter
{
    void Set(EventEnvelopeContext context);
}
