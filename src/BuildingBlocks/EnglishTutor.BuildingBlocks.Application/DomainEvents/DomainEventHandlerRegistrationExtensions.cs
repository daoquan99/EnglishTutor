using EnglishTutor.BuildingBlocks.Domain.DomainEvents;
using Microsoft.Extensions.DependencyInjection;

namespace EnglishTutor.BuildingBlocks.Application.DomainEvents;

// Per-event handler DI registration. The dispatcher (concrete
// IDomainEventDispatcher implementation) lives in BuildingBlocks.Infrastructure;
// the dispatcher DI extension lives there too. This file only
// exposes the per-event handler registration so module infrastructure
// projects can register their IDomainEventHandler<TEvent> implementations
// without depending on the dispatcher.
public static class DomainEventHandlerRegistrationExtensions
{
    public static IServiceCollection AddDomainEventHandler<TEvent, THandler>(
        this IServiceCollection services)
        where TEvent : IDomainEvent
        where THandler : class, IDomainEventHandler<TEvent>
    {
        services.AddScoped<IDomainEventHandler<TEvent>, THandler>();
        return services;
    }
}
