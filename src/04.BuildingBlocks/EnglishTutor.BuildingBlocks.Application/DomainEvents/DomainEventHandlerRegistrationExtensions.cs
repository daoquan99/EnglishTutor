using EnglishTutor.BuildingBlocks.Domain.DomainEvents;
using Microsoft.Extensions.DependencyInjection;

namespace EnglishTutor.BuildingBlocks.Application.DomainEvents;

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
