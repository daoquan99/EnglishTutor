using EnglishTutor.Audit.Presentation.Endpoints;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EnglishTutor.Audit.Presentation;

public static class AuditPresentationServiceCollectionExtensions
{
    public static IServiceCollection AddAuditPresentation(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        return services;
    }

    public static IEndpointRouteBuilder MapAuditEndpoints(this IEndpointRouteBuilder routes)
    {
        routes.RegisterAuditEndpoints();
        return routes;
    }
}
