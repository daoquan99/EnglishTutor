using System;
using System.Threading;
using EnglishTutor.BuildingBlocks.Application.DomainEvents;

namespace EnglishTutor.BuildingBlocks.Infrastructure.DomainEvents;

/// <summary>
/// Infrastructure implementation of the event envelope context accessor using AsyncLocal.
/// </summary>
public sealed class EventEnvelopeContextAccessor : IEventEnvelopeContextAccessor, IEventEnvelopeContextSetter
{
    private static readonly AsyncLocal<EventEnvelopeContext?> Context = new();

    public EventEnvelopeContext Current
    {
        get
        {
            if (Context.Value == null)
            {
                Context.Value = new EventEnvelopeContext(
                    CorrelationId: Guid.NewGuid(),
                    CausationId: null);
            }
            return Context.Value;
        }
    }

    public void Set(EventEnvelopeContext context)
    {
        Context.Value = context ?? throw new ArgumentNullException(nameof(context));
    }
}
