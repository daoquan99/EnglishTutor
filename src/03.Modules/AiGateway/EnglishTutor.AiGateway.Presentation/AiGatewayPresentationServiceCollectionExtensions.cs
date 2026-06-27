using Microsoft.AspNetCore.Routing;

namespace EnglishTutor.AiGateway.Presentation;

public interface IAiGatewayPresentationMarker { }

public static class AiGatewayPresentationServiceCollectionExtensions
{
    public static IEndpointRouteBuilder MapAiGatewayEndpoints(this IEndpointRouteBuilder routes)
    {
        Endpoints.AiGatewayAdminEndpoints.MapAiGatewayAdminEndpoints(routes);
        return routes;
    }
}
