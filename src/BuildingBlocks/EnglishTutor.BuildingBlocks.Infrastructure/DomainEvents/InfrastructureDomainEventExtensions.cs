using EnglishTutor.BuildingBlocks.Application.DomainEvents;
using EnglishTutor.BuildingBlocks.Domain.DomainEvents;
using Microsoft.Extensions.DependencyInjection;

namespace EnglishTutor.BuildingBlocks.Infrastructure.DomainEvents;

// DI extension methods for the in-process domain event dispatcher.
//
// BuildingBlocks.Application exposes only the IDomainEventDispatcher
// interface and IDomainEventHandler<TEvent>; the concrete implementation
// and this extension live in BuildingBlocks.Infrastructure so that
// BuildingBlocks.Application does not take a compile-time dependency on
// BuildingBlocks.Infrastructure (a layer inversion).
//
// Module composition roots call AddDomainEventDispatcher() explicitly.
// Each IDomainEventHandler<TEvent> is registered through
// AddDomainEventHandler<TEvent, THandler>() from the owning module's
// Infrastructure project.
public static class InfrastructureDomainEventExtensions
{
    public static IServiceCollection AddDomainEventDispatcher(this IServiceCollection services)
    {
        if (services.Any(s => s.ServiceType == typeof(IDomainEventDispatcher)))
        {
            return services;
        }

        services.AddScoped<IDomainEventDispatcher, InMemoryDomainEventDispatcher>();
        return services;
    }
}
