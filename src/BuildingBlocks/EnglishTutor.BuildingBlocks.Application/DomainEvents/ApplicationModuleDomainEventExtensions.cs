using System.Reflection;
using EnglishTutor.BuildingBlocks.Domain.DomainEvents;
using Microsoft.Extensions.DependencyInjection;

namespace EnglishTutor.BuildingBlocks.Application.DomainEvents;

// DI extension methods for the in-process domain event dispatcher and
// per-event handlers. Lives in BuildingBlocks.Application next to
// ApplicationModuleExtensions; BuildingBlocks.Domain is a pure abstraction
// layer and intentionally has no IServiceCollection extension.
//
// The concrete IDomainEventDispatcher implementation lives in
// BuildingBlocks.Infrastructure. We resolve it by reflection so that
// BuildingBlocks.Application does not take a compile-time dependency on
// BuildingBlocks.Infrastructure (a layer inversion).
public static class ApplicationModuleDomainEventExtensions
{
    public static IServiceCollection AddDomainEventDispatcher(this IServiceCollection services)
    {
        if (services.Any(s => s.ServiceType == typeof(IDomainEventDispatcher)))
        {
            return services;
        }

        Assembly? infrastructureAssembly = null;
        try
        {
            infrastructureAssembly = AppDomain.CurrentDomain.GetAssemblies()
                .FirstOrDefault(a => a.GetName().Name == "EnglishTutor.BuildingBlocks.Infrastructure")
                ?? Assembly.Load("EnglishTutor.BuildingBlocks.Infrastructure");
        }
        catch
        {
            // Fail-silent here; the null-coalescing check below will throw a descriptive exception.
        }

        if (infrastructureAssembly is null)
        {
            throw new InvalidOperationException(
                "EnglishTutor.BuildingBlocks.Infrastructure assembly reference is required " +
                "to register the default IDomainEventDispatcher. Add a ProjectReference " +
                "from your module's Infrastructure project to EnglishTutor.BuildingBlocks.Infrastructure, " +
                "or call AddScoped<IDomainEventDispatcher, MyCustomDispatcher>() before AddDomainEventDispatcher().");
        }

        var dispatcherType = infrastructureAssembly
            .GetTypes()
            .FirstOrDefault(t => !t.IsAbstract && !t.IsInterface
                && typeof(IDomainEventDispatcher).IsAssignableFrom(t))
            ?? throw new InvalidOperationException(
                "Could not locate a concrete IDomainEventDispatcher implementation in " +
                "EnglishTutor.BuildingBlocks.Infrastructure. " +
                "Register one explicitly with AddScoped<IDomainEventDispatcher, T>() before AddDomainEventDispatcher().");

        services.AddScoped(typeof(IDomainEventDispatcher), dispatcherType);
        return services;
    }

    public static IServiceCollection AddDomainEventHandler<TEvent, THandler>(
        this IServiceCollection services)
        where TEvent : IDomainEvent
        where THandler : class, IDomainEventHandler<TEvent>
    {
        services.AddScoped<IDomainEventHandler<TEvent>, THandler>();
        return services;
    }
}
