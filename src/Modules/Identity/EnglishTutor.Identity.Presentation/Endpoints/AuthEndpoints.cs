using EnglishTutor.BuildingBlocks.Application.CurrentUser;
using EnglishTutor.BuildingBlocks.Domain.Results;
using EnglishTutor.Identity.Application.Abstractions;
using EnglishTutor.Identity.Application.Commands.Login;
using EnglishTutor.Identity.Application.Commands.LogoutAll;
using EnglishTutor.Identity.Application.Commands.RevokeSession;
using EnglishTutor.Identity.Application.Commands.Refresh;
using EnglishTutor.Identity.Application.Queries.GetUserSessions;
using EnglishTutor.Identity.Presentation.Auth;
using EnglishTutor.Identity.Presentation.Endpoints.Dtos;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Options;
using System;
using System.Threading;
using System.Threading.Tasks;

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
            IOptions<AuthCookieOptions> cookieOptions,
            CancellationToken ct) =>
        {
            var options = cookieOptions.Value;
            var userAgent = httpContext.Request.Headers.UserAgent.ToString();
            var ipAddress = httpContext.Connection.RemoteIpAddress?.ToString();
            var deviceId = httpContext.Request.Headers["X-Device-Id"].ToString();
            var deviceName = httpContext.Request.Headers["X-Device-Name"].ToString();

            var command = new LoginCommand(
                request.Email,
                request.Password,
                IpAddress: ipAddress,
                UserAgent: string.IsNullOrWhiteSpace(userAgent) ? null : userAgent,
                DeviceId: string.IsNullOrWhiteSpace(deviceId) ? null : deviceId,
                DeviceName: string.IsNullOrWhiteSpace(deviceName) ? null : deviceName);

            var result = await mediator.Send(command, ct);
            if (!result.IsSuccess)
            {
                return MapErrorToHttp(result.Error!);
            }

            // Transport refresh token in secure HttpOnly cookie.
            RefreshTokenCookieHelper.SetRefreshTokenCookie(
                httpContext.Response,
                result.Value!.RefreshToken,
                result.Value.ExpiresAt,
                options);

            return Results.Ok(ToLoginResponse(result.Value!));
        });

        group.MapPost("/refresh", async (
            RefreshRequest? request,
            IMediator mediator,
            HttpContext httpContext,
            IOptions<AuthCookieOptions> cookieOptions,
            CancellationToken ct) =>
        {
            var options = cookieOptions.Value;
            var token = httpContext.Request.Cookies[options.RefreshTokenCookieName];
            bool fromCookie = !string.IsNullOrWhiteSpace(token);

            if (!fromCookie && request is not null)
            {
                token = request.RefreshToken;
            }

            if (string.IsNullOrWhiteSpace(token))
            {
                return Results.Unauthorized();
            }

            if (fromCookie && !ValidateCsrf(httpContext.Request))
            {
                return Results.BadRequest("CSRF validation failed.");
            }

            var command = new RefreshCommand(
                token,
                IpAddress: httpContext.Connection.RemoteIpAddress?.ToString());

            var result = await mediator.Send(command, ct);
            if (!result.IsSuccess)
            {
                // On refresh failure, clear the cookie.
                if (fromCookie)
                {
                    RefreshTokenCookieHelper.ClearRefreshTokenCookie(httpContext.Response, options);
                }
                return MapErrorToHttp(result.Error!);
            }

            // Rotate: set new refresh cookie.
            RefreshTokenCookieHelper.SetRefreshTokenCookie(
                httpContext.Response,
                result.Value!.RefreshToken,
                result.Value.ExpiresAt,
                options);

            return Results.Ok(ToRefreshResponse(result.Value!));
        });

        group.MapPost("/logout", async (
            RefreshRequest? request,
            IMediator mediator,
            HttpContext httpContext,
            IOptions<AuthCookieOptions> cookieOptions,
            CancellationToken ct) =>
        {
            var options = cookieOptions.Value;
            var token = httpContext.Request.Cookies[options.RefreshTokenCookieName];
            bool fromCookie = !string.IsNullOrWhiteSpace(token);

            if (!fromCookie && request is not null)
            {
                token = request.RefreshToken;
            }

            if (string.IsNullOrWhiteSpace(token))
            {
                // Idempotent: succeed silently if no token is found.
                return Results.NoContent();
            }

            if (fromCookie && !ValidateCsrf(httpContext.Request))
            {
                return Results.BadRequest("CSRF validation failed.");
            }

            var command = new Application.Commands.Logout.LogoutCommand(token);
            var result = await mediator.Send(command, ct);

            if (fromCookie)
            {
                RefreshTokenCookieHelper.ClearRefreshTokenCookie(httpContext.Response, options);
            }

            return result.IsSuccess ? Results.NoContent() : MapErrorToHttp(result.Error!);
        }).RequireAuthorization();

        group.MapPost("/logout-all", async (
            IMediator mediator,
            ICurrentUser currentUser,
            HttpContext httpContext,
            IOptions<AuthCookieOptions> cookieOptions,
            CancellationToken ct) =>
        {
            var userId = currentUser.UserId;
            if (!userId.HasValue)
            {
                return Results.Unauthorized();
            }

            var command = new LogoutAllCommand(userId.Value);
            var result = await mediator.Send(command, ct);
            if (!result.IsSuccess)
            {
                return MapErrorToHttp(result.Error!);
            }

            // Clear refresh cookie.
            RefreshTokenCookieHelper.ClearRefreshTokenCookie(httpContext.Response, cookieOptions.Value);

            return Results.NoContent();
        }).RequireAuthorization();

        group.MapGet("/sessions", async (
            IMediator mediator,
            ICurrentUser currentUser,
            CancellationToken ct) =>
        {
            var userId = currentUser.UserId;
            if (!userId.HasValue)
            {
                return Results.Unauthorized();
            }

            var query = new GetUserSessionsQuery(userId.Value);
            var result = await mediator.Send(query, ct);
            if (!result.IsSuccess)
            {
                return MapErrorToHttp(result.Error!);
            }

            var response = result.Value!.Select(ToUserSessionResponse).ToList();
            return Results.Ok(response);
        }).RequireAuthorization();

        group.MapDelete("/sessions/{sessionId:guid}", async (
            Guid sessionId,
            IMediator mediator,
            ICurrentUser currentUser,
            CancellationToken ct) =>
        {
            var userId = currentUser.UserId;
            if (!userId.HasValue)
            {
                return Results.Unauthorized();
            }

            bool isAdmin = currentUser.IsInRole("Admin") || currentUser.IsInRole("Owner");
            var command = new RevokeSessionCommand(sessionId, userId.Value, isAdmin);
            var result = await mediator.Send(command, ct);

            return result.IsSuccess ? Results.NoContent() : MapErrorToHttp(result.Error!);
        }).RequireAuthorization();

        return routes;
    }

    private static bool ValidateCsrf(HttpRequest request)
    {
        // Require a custom CSRF request header when using cookies.
        return request.Headers.ContainsKey("X-Requested-With") ||
               request.Headers.ContainsKey("X-XSRF-TOKEN") ||
               request.Headers.ContainsKey("X-XSRF-Header");
    }

    // ----- Application result -> Presentation DTO mapping (HTTP boundary) -----
    private static LoginResponse ToLoginResponse(LoginResult r) =>
        new(r.AccessToken, r.RefreshToken, r.ExpiresAt);

    private static RefreshResponse ToRefreshResponse(RefreshResult r) =>
        new(r.AccessToken, r.RefreshToken, r.ExpiresAt);

    private static UserSessionResponse ToUserSessionResponse(UserSessionResult s) =>
        new(s.Id, s.DeviceId, s.DeviceName, s.UserAgentHash, s.IpAddressHash, s.CreatedAtUtc, s.LastSeenAtUtc);

    private static IResult MapErrorToHttp(EnglishTutor.BuildingBlocks.Domain.Results.Error error)
    {
        return error.Code switch
        {
            "Identity.InvalidCredentials"   => Results.Unauthorized(),
            "Identity.AccountLocked"        => Results.Unauthorized(),
            "Identity.AccountInactive"      => Results.Unauthorized(),
            "Identity.RefreshTokenReuse"    => Results.Unauthorized(),
            "Identity.InvalidRefreshToken"  => Results.Unauthorized(),
            "Identity.SessionNotFound"      => Results.NotFound(error.Message),
            _ => Results.Problem(detail: error.Message, statusCode: 400, title: error.Code)
        };
    }
}
