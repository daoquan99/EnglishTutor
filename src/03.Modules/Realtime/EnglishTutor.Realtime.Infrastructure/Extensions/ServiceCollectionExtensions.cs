using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using EnglishTutor.BuildingBlocks.Application;
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
        services.AddLicensedMediatR(configuration["MediatR:LicenseKey"], typeof(RealtimeApplicationServiceCollectionExtensions).Assembly);

        return services;
    }
}
