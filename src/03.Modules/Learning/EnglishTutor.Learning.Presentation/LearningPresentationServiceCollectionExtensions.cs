using EnglishTutor.Learning.Presentation.Endpoints;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EnglishTutor.Learning.Presentation;

public static class LearningPresentationServiceCollectionExtensions
{
    public static IServiceCollection AddLearningPresentation(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddHttpContextAccessor();
        return services;
    }

    public static IEndpointRouteBuilder MapLearningEndpoints(this IEndpointRouteBuilder routes)
    {
        routes.MapTopicsEndpoints();
        routes.MapModesEndpoints();
        routes.MapScenariosEndpoints();
        routes.MapVocabularyEndpoints();
        routes.MapPhrasesEndpoints();
        return routes;
    }
}
