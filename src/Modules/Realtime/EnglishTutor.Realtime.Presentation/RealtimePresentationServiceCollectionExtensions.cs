using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using EnglishTutor.Realtime.Application.Abstractions;
using EnglishTutor.Realtime.Presentation.Hubs;
using EnglishTutor.Realtime.Presentation.Notifications;

namespace EnglishTutor.Realtime.Presentation;

public static class RealtimePresentationServiceCollectionExtensions
{
    public static IServiceCollection AddRealtimePresentation(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddSignalR();
        services.AddScoped<IRealtimeNotifier, RealtimeNotifier>();
        return services;
    }

    public static IEndpointRouteBuilder MapRealtimeHubs(this IEndpointRouteBuilder routes)
    {
        routes.MapHub<PracticeHub>("/hubs/practice");
        return routes;
    }
}
