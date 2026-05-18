using EnglishTutor.BuildingBlocks.Presentation;
using EnglishTutor.Modules.Auth.Application.DTOs;
using EnglishTutor.Modules.Auth.Application.Errors;
using EnglishTutor.BuildingBlocks.Application.Results;
using EnglishTutor.Modules.Auth.Application.Commands.Login;
using EnglishTutor.Modules.Auth.Application.Commands.Logout;
using EnglishTutor.Modules.Auth.Application.Commands.RefreshToken;
using EnglishTutor.Modules.Auth.Application.Commands.Register;
using EnglishTutor.Modules.Auth.Application.Queries.GetCurrentUser;
using EnglishTutor.Modules.Auth.Presentation.Requests;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace EnglishTutor.Modules.Auth.Presentation;

public static class AuthEndpoints
{
    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/auth").WithTags("Auth");

        group.MapPost("/register", async (RegisterRequest request, HttpContext httpContext, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new RegisterCommand(
                request.Email,
                request.Password,
                request.ConfirmPassword,
                request.DisplayName,
                AuthCookieManager.GetOrCreateDeviceId(httpContext.Request),
                httpContext.Request.Headers.UserAgent.ToString(),
                httpContext.Connection.RemoteIpAddress?.ToString()), ct);

            if (result.IsSuccess)
            {
                AuthCookieManager.SetAuthCookies(httpContext.Response, result.Value);
            }

            return result.ToCreatedResult();
        })
            .AllowAnonymous()
            .WithName("Register");

        group.MapPost("/login", async (LoginRequest request, HttpContext httpContext, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new LoginCommand(
                request.Email,
                request.Password,
                AuthCookieManager.GetOrCreateDeviceId(httpContext.Request),
                httpContext.Request.Headers.UserAgent.ToString(),
                httpContext.Connection.RemoteIpAddress?.ToString()), ct);

            if (result.IsSuccess)
            {
                AuthCookieManager.SetAuthCookies(httpContext.Response, result.Value);
            }

            return result.ToHttpResult();
        })
            .AllowAnonymous()
            .WithName("Login");

        group.MapPost("/refresh-token", async (HttpContext httpContext, ISender sender, CancellationToken ct) =>
        {
            if (!AuthCookieManager.TryReadRefreshCookies(httpContext.Request, out var refreshToken, out var sessionId, out var deviceId))
            {
                AuthCookieManager.ClearAuthCookies(httpContext.Response);
                return Result.Failure<AuthTokenResponse>(AuthErrors.RefreshTokenNotFound).ToHttpResult();
            }

            var result = await sender.Send(new RefreshTokenCommand(
                refreshToken,
                sessionId,
                deviceId,
                httpContext.Request.Headers.UserAgent.ToString(),
                httpContext.Connection.RemoteIpAddress?.ToString()), ct);

            if (result.IsSuccess)
            {
                AuthCookieManager.SetAuthCookies(httpContext.Response, result.Value);
            }
            else
            {
                AuthCookieManager.ClearAuthCookies(httpContext.Response);
            }

            return result.ToHttpResult();
        })
            .AllowAnonymous()
            .WithName("RefreshToken");

        group.MapPost("/logout", async (HttpContext httpContext, ISender sender, CancellationToken ct) =>
        {
            if (!AuthCookieManager.TryReadRefreshCookies(httpContext.Request, out var refreshToken, out var sessionId, out var deviceId))
            {
                AuthCookieManager.ClearAuthCookies(httpContext.Response);
                return Result.Failure(AuthErrors.RefreshTokenNotFound).ToHttpResult();
            }

            var userIdClaim = httpContext.User.FindFirst("sub")?.Value
                ?? httpContext.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userIdClaim, out var userId))
            {
                AuthCookieManager.ClearAuthCookies(httpContext.Response);
                return Result.Failure(AuthErrors.UserNotFound(Guid.Empty)).ToHttpResult();
            }

            var result = await sender.Send(new LogoutCommand(userId, refreshToken, sessionId, deviceId), ct);
            AuthCookieManager.ClearAuthCookies(httpContext.Response);
            return result.ToHttpResult();
        })
            .RequireAuthorization()
            .WithName("Logout");

        group.MapGet("/me", async (ISender sender, CancellationToken ct) =>
            (await sender.Send(new GetCurrentUserQuery(), ct)).ToHttpResult())
            .RequireAuthorization()
            .WithName("GetCurrentUser");

        return endpoints;
    }
}
