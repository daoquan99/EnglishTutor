using EnglishTutor.Practice.Presentation.Endpoints;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EnglishTutor.Practice.Presentation;

public static class PracticePresentationServiceCollectionExtensions
{
    public static IServiceCollection AddPracticePresentation(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddHttpContextAccessor();
        return services;
    }

    public static IEndpointRouteBuilder MapPracticeModuleEndpoints(this IEndpointRouteBuilder routes)
    {
        routes.MapPracticeEndpoints();
        return routes;
    }
}
