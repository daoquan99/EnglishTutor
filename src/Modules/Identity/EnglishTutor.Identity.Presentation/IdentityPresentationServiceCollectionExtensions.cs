using EnglishTutor.Identity.Presentation.Endpoints;
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

        return services;
    }

    public static IEndpointRouteBuilder MapIdentityEndpoints(this IEndpointRouteBuilder routes)
    {
        routes.MapAuthEndpoints();
        routes.MapMeEndpoints();
        return routes;
    }
}
