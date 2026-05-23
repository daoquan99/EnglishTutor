namespace EnglishTutor.BuildingBlocks.Domain;

public interface IDomainEventHolder
{
    IReadOnlyList<DomainEvent> DomainEvents { get; }
    void ClearDomainEvents();
}
