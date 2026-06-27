using System;
using System.Threading.Tasks;
using EnglishTutor.BuildingBlocks.Application.DomainEvents;
using MassTransit;

namespace EnglishTutor.BuildingBlocks.Infrastructure.Messaging;

/// <summary>
/// MassTransit consume filter to populate host-neutral event context for incoming message consumers.
/// </summary>
public sealed class EventEnvelopeContextFilter<T> : IFilter<ConsumeContext<T>> where T : class
{
    private readonly IEventEnvelopeContextSetter _contextSetter;

    public EventEnvelopeContextFilter(IEventEnvelopeContextSetter contextSetter)
    {
        _contextSetter = contextSetter ?? throw new ArgumentNullException(nameof(contextSetter));
    }

    public void Probe(ProbeContext context)
    {
        context.CreateFilterScope("event-envelope-context");
    }

    public async Task Send(ConsumeContext<T> context, IPipe<ConsumeContext<T>> next)
    {
        var correlationId = context.CorrelationId ?? Guid.NewGuid();
        var causationId = context.MessageId;

        _contextSetter.Set(new EventEnvelopeContext(
            CorrelationId: correlationId,
            CausationId: causationId));

        await next.Send(context);
    }
}
