using EnglishTutor.Identity.Presentation.Endpoints;
using EnglishTutor.Identity.Presentation.RateLimiting;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace EnglishTutor.Identity.Presentation;

public static class IdentityPresentationServiceCollectionExtensions
{
    public static IServiceCollection AddIdentityPresentation(
        this IServiceCollection services,
        Microsoft.Extensions.Configuration.IConfiguration configuration)
    {
        services.AddHttpContextAccessor();
        services.AddScoped<EnglishTutor.BuildingBlocks.Application.CurrentUser.ICurrentUser,
            Auth.HttpContextCurrentUser>();

        services
            .AddOptions<Auth.AuthCookieOptions>()
            .Bind(configuration.GetSection(Auth.AuthCookieOptions.SectionName))
            .ValidateOnStart();

        services
            .AddOptions<Auth.CsrfOptions>()
            .Bind(configuration.GetSection(Auth.CsrfOptions.SectionName))
            .ValidateOnStart();

        // In-process auth rate-limiter policies + 429 ProblemDetails (H-06).
        services.AddIdentityRateLimiting(configuration);

        return services;
    }

    public static IEndpointRouteBuilder MapIdentityEndpoints(this IEndpointRouteBuilder routes)
    {
        routes.MapAuthEndpoints();
        routes.MapMeEndpoints();
        routes.MapMeProfileEndpoints();
        routes.MapAdminIdentityEndpoints();
        return routes;
    }
}
