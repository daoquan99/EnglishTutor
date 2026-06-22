using EnglishTutor.BuildingBlocks.Domain.Results;
using EnglishTutor.Identity.Application.Abstractions;
using EnglishTutor.Identity.Application.Commands.Login;
using EnglishTutor.Identity.Application.Commands.Refresh;
using EnglishTutor.Identity.Presentation.Endpoints.Dtos;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace EnglishTutor.Identity.Presentation.Endpoints;

public static class AuthEndpoints
{
    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/auth").WithTags("Auth");

        group.MapPost("/login", async (
            LoginRequest request,
            IMediator mediator,
            HttpContext httpContext,
            CancellationToken ct) =>
        {
            var command = new LoginCommand(
                request.Email,
                request.Password,
                IpAddress: httpContext.Connection.RemoteIpAddress?.ToString());

            var result = await mediator.Send(command, ct);
            if (!result.IsSuccess)
            {
                return MapErrorToHttp(result.Error!);
            }
            // IsSuccess guarantees Value is non-null (per Result<T> contract).
            return Results.Ok(ToLoginResponse(result.Value!));
        });

        group.MapPost("/refresh", async (
            RefreshRequest request,
            IMediator mediator,
            HttpContext httpContext,
            CancellationToken ct) =>
        {
            var command = new RefreshCommand(
                request.RefreshToken,
                IpAddress: httpContext.Connection.RemoteIpAddress?.ToString());

            var result = await mediator.Send(command, ct);
            if (!result.IsSuccess)
            {
                return MapErrorToHttp(result.Error!);
            }
            // IsSuccess guarantees Value is non-null (per Result<T> contract).
            return Results.Ok(ToRefreshResponse(result.Value!));
        });

        group.MapPost("/logout", async (
            RefreshRequest request,
            IMediator mediator,
            CancellationToken ct) =>
        {
            var command = new Application.Commands.Logout.LogoutCommand(request.RefreshToken);
            var result = await mediator.Send(command, ct);
            return result.IsSuccess ? Results.NoContent() : MapErrorToHttp(result.Error!);
        }).RequireAuthorization();

        return routes;
    }

    // ----- Application result -> Presentation DTO mapping (HTTP boundary) -----
    private static LoginResponse ToLoginResponse(LoginResult r) =>
        new(r.AccessToken, r.RefreshToken, r.ExpiresAt);

    private static RefreshResponse ToRefreshResponse(RefreshResult r) =>
        new(r.AccessToken, r.RefreshToken, r.ExpiresAt);

    private static IResult MapErrorToHttp(EnglishTutor.BuildingBlocks.Domain.Results.Error error)
    {
        return error.Code switch
        {
            "Identity.InvalidCredentials"   => Results.Unauthorized(),
            "Identity.AccountLocked"        => Results.Unauthorized(),
            "Identity.AccountInactive"      => Results.Unauthorized(),
            "Identity.RefreshTokenReuse"    => Results.Unauthorized(),
            "Identity.InvalidRefreshToken"  => Results.Unauthorized(),
            _ => Results.Problem(detail: error.Message, statusCode: 400, title: error.Code)
        };
    }
}
