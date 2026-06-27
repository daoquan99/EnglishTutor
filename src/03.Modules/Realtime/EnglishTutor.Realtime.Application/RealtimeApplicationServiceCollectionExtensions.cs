using Microsoft.Extensions.DependencyInjection;
using FluentValidation;

namespace EnglishTutor.Realtime.Application;

public static class RealtimeApplicationServiceCollectionExtensions
{
    public static IServiceCollection AddRealtimeApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssembly(typeof(RealtimeApplicationServiceCollectionExtensions).Assembly, includeInternalTypes: true);
        return services;
    }
}
