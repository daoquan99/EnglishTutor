using EnglishTutor.Feedback.Presentation.Endpoints;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EnglishTutor.Feedback.Presentation;

public static class FeedbackPresentationServiceCollectionExtensions
{
    public static IServiceCollection AddFeedbackPresentation(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddHttpContextAccessor();
        return services;
    }

    public static IEndpointRouteBuilder MapFeedbackModuleEndpoints(this IEndpointRouteBuilder routes)
    {
        routes.MapFeedbackEndpoints();
        return routes;
    }
}
