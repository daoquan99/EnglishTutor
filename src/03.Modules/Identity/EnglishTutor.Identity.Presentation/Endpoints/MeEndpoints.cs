using EnglishTutor.BuildingBlocks.Application.CurrentUser;
using EnglishTutor.BuildingBlocks.Domain.Results;
using EnglishTutor.BuildingBlocks.Presentation.Responses;
using EnglishTutor.Identity.Application.Queries.GetCurrentUser;
using EnglishTutor.Identity.Presentation.Endpoints.Dtos;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace EnglishTutor.Identity.Presentation.Endpoints;

public static class MeEndpoints
{
    public static IEndpointRouteBuilder MapMeEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/me").WithTags("Me").RequireAuthorization();

        group.MapGet("/", async (
            IMediator mediator,
            CancellationToken ct) =>
        {
            var result = await mediator.Send(new GetCurrentUserQuery(), ct);
            if (!result.IsSuccess)
            {
                return Auth.AuthProblemResults.Unauthorized();
            }
            // IsSuccess guarantees Value is non-null (per Result<T> contract).
            return ApiResults.Ok(ToCurrentUserResponse(result.Value!));
        });

        return routes;
    }

    // ----- Application result -> Presentation DTO mapping (HTTP boundary) -----
    private static CurrentUserResponse ToCurrentUserResponse(CurrentUserResult r) =>
        new(r.Id, r.Email, r.DisplayName, r.Roles);
}
