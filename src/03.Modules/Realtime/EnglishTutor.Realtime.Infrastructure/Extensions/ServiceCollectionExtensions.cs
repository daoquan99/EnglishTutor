using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using EnglishTutor.Realtime.Application;
using EnglishTutor.Realtime.Application.Abstractions;
using EnglishTutor.Realtime.Infrastructure.Registry;

namespace EnglishTutor.Realtime.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddRealtimeInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddSingleton<IRealtimeConnectionRegistry, InMemoryRealtimeConnectionRegistry>();
        
        // Add MediatR from Application assembly
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(RealtimeApplicationServiceCollectionExtensions).Assembly));

        return services;
    }
}
