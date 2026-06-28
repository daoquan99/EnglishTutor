using EnglishTutor.BuildingBlocks.Application.CurrentUser;
using EnglishTutor.BuildingBlocks.Presentation.Responses;
using EnglishTutor.Identity.Application.Users.Commands.ChangeMyPassword;
using EnglishTutor.Identity.Application.Users.Commands.UpdateMyProfile;
using EnglishTutor.Identity.Presentation.Endpoints.Dtos;
using EnglishTutor.Identity.Presentation.RateLimiting;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace EnglishTutor.Identity.Presentation.Endpoints;

public static class MeProfileEndpoints
{
    public static IEndpointRouteBuilder MapMeProfileEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/me").WithTags("Me").RequireAuthorization();

        group.MapPut("/profile", async (
            UpdateMyProfileRequest request,
            IMediator mediator,
            ICurrentUser currentUser,
            CancellationToken ct) =>
        {
            if (!currentUser.UserId.HasValue)
            {
                return ApiResults.Problem(
                    statusCode: StatusCodes.Status401Unauthorized,
                    code: ApiErrorCodes.Unauthorized,
                    title: "Unauthorized",
                    message: "Authentication is required.");
            }

            var result = await mediator.Send(new UpdateMyProfileCommand(
                UserId: currentUser.UserId.Value,
                DisplayName: request.DisplayName), ct);

            return result.IsSuccess
                ? ApiResults.Ok(new CurrentUserResponse(
                    Id: result.Value!.Id,
                    Email: result.Value.Email,
                    DisplayName: result.Value.DisplayName,
                    Roles: result.Value.Roles))
                : ApiResults.FromError(result.Error!);
        }).RequireRateLimiting(AuthRateLimitPolicies.SessionMutation);

        group.MapPut("/password", async (
            ChangeMyPasswordRequest request,
            IMediator mediator,
            ICurrentUser currentUser,
            CancellationToken ct) =>
        {
            if (!currentUser.UserId.HasValue)
            {
                return ApiResults.Problem(
                    statusCode: StatusCodes.Status401Unauthorized,
                    code: ApiErrorCodes.Unauthorized,
                    title: "Unauthorized",
                    message: "Authentication is required.");
            }

            var result = await mediator.Send(new ChangeMyPasswordCommand(
                UserId: currentUser.UserId.Value,
                CurrentPassword: request.CurrentPassword,
                NewPassword: request.NewPassword), ct);

            return result.IsSuccess ? ApiResults.Empty() : ApiResults.FromError(result.Error!);
        }).RequireRateLimiting(AuthRateLimitPolicies.SessionMutation);

        return routes;
    }
}
