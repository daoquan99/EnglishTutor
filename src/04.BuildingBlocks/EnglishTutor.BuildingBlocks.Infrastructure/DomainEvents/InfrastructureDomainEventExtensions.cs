using EnglishTutor.BuildingBlocks.Application.DomainEvents;
using EnglishTutor.BuildingBlocks.Domain.DomainEvents;
using Microsoft.Extensions.DependencyInjection;

namespace EnglishTutor.BuildingBlocks.Infrastructure.DomainEvents;

public static class InfrastructureDomainEventExtensions
{
    public static IServiceCollection AddDomainEventDispatcher(this IServiceCollection services)
    {
        if (!services.Any(s => s.ServiceType == typeof(IDomainEventDispatcher)))
        {
            services.AddScoped<IDomainEventDispatcher, InMemoryDomainEventDispatcher>();
        }

        if (!services.Any(s => s.ServiceType == typeof(ITransactionalDomainEventDispatcher)))
        {
            services.AddScoped<ITransactionalDomainEventDispatcher, TransactionalDomainEventDispatcher>();
        }

        services.AddSingleton<EventEnvelopeContextAccessor>();
        services.AddSingleton<IEventEnvelopeContextAccessor>(sp => sp.GetRequiredService<EventEnvelopeContextAccessor>());
        services.AddSingleton<IEventEnvelopeContextSetter>(sp => sp.GetRequiredService<EventEnvelopeContextAccessor>());

        return services;
    }
}
