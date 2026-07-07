using EnglishTutor.BuildingBlocks.Application.CurrentUser;
using EnglishTutor.BuildingBlocks.Presentation.Responses;
using EnglishTutor.Progress.Application.Progress.Queries.GetProgressSummary;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace EnglishTutor.Progress.Presentation;

public static class ProgressPresentationExtensions
{
    public static IServiceCollection AddProgressPresentation(this IServiceCollection services) =>
        services;

    public static IEndpointRouteBuilder MapProgressEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/progress")
            .WithTags("Progress")
            .RequireAuthorization();
        group.MapGet("/summary", async (
            Guid? languagePairId,
            ICurrentUser currentUser,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(
                new GetProgressSummaryQuery(currentUser.UserId!.Value, languagePairId),
                cancellationToken);
            return ApiResults.Ok(result.Value);
        });
        return routes;
    }
}
