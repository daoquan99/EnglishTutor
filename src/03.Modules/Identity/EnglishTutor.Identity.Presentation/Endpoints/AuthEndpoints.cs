using EnglishTutor.BuildingBlocks.Application.CurrentUser;
using EnglishTutor.BuildingBlocks.Presentation.Responses;
using EnglishTutor.Identity.Application.Abstractions;
using EnglishTutor.Identity.Application.Commands.Login;
using EnglishTutor.Identity.Application.Commands.LogoutAll;
using EnglishTutor.Identity.Application.Commands.RevokeSession;
using EnglishTutor.Identity.Application.Commands.Refresh;
using EnglishTutor.Identity.Application.Queries.GetUserSessions;
using EnglishTutor.Identity.Presentation.Auth;
using EnglishTutor.Identity.Presentation.Endpoints.Dtos;
using EnglishTutor.Identity.Presentation.RateLimiting;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Options;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace EnglishTutor.Identity.Presentation.Endpoints;

public static class AuthEndpoints
{
    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/auth").WithTags("Auth");

        // ----- CSRF bootstrap (safe GET) -----
        // Issues the double-submit CSRF cookie and returns the token value for
        // the client to echo in the X-CSRF-TOKEN header on mutation calls.
        group.MapGet("/csrf", (HttpContext httpContext, IOptions<CsrfOptions> csrfOptions) =>
        {
            var token = CsrfProtection.IssueToken(httpContext.Response, csrfOptions.Value);
            return ApiResults.Ok(new CsrfTokenResponse(token));
        });

        // ----- Login -----
        group.MapPost("/login", async (
            LoginRequest request,
            IMediator mediator,
            HttpContext httpContext,
            IOptions<AuthCookieOptions> cookieOptions,
            IOptions<CsrfOptions> csrfOptions,
            CancellationToken ct) =>
        {
            var options = cookieOptions.Value;
            var userAgent = httpContext.Request.Headers.UserAgent.ToString();
            var ipAddress = httpContext.Connection.RemoteIpAddress?.ToString();
            var deviceId = httpContext.Request.Headers[IdentityEndpointHeaders.DeviceId].ToString();
            var deviceName = httpContext.Request.Headers[IdentityEndpointHeaders.DeviceName].ToString();

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
                return AuthProblemResults.FromError(result.Error!);
            }

            // Refresh token is transported ONLY via the HttpOnly cookie (H-01).
            RefreshTokenCookieHelper.SetRefreshTokenCookie(
                response: httpContext.Response,
                rawRefreshToken: result.Value!.RefreshToken,
                expiresAtUtc: result.Value.RefreshTokenExpiresAt,
                options: options);

            // Issue a CSRF token so the client can immediately call refresh.
            CsrfProtection.IssueToken(httpContext.Response, csrfOptions.Value);

            return ApiResults.Ok(ToLoginResponse(result.Value!));
        }).RequireRateLimiting(AuthRateLimitPolicies.Login);

        // ----- Refresh (cookie-only + CSRF) -----
        group.MapPost("/refresh", async (
            IMediator mediator,
            HttpContext httpContext,
            IOptions<AuthCookieOptions> cookieOptions,
            IOptions<CsrfOptions> csrfOptions,
            CancellationToken ct) =>
        {
            var options = cookieOptions.Value;
            var token = httpContext.Request.Cookies[options.RefreshTokenCookieName];

            // Browser-only transport: no request-body fallback (H-01/H-03).
            if (string.IsNullOrWhiteSpace(token))
            {
                return AuthProblemResults.RefreshCookieMissing();
            }

            var csrfError = ValidateCsrf(httpContext, csrfOptions.Value);
            if (csrfError is not null)
            {
                return csrfError;
            }

            var command = new RefreshCommand(
                token,
                IpAddress: httpContext.Connection.RemoteIpAddress?.ToString());

            var result = await mediator.Send(command, ct);
            if (!result.IsSuccess)
            {
                // On any refresh failure, clear the cookie.
                RefreshTokenCookieHelper.ClearRefreshTokenCookie(httpContext.Response, options);
                return AuthProblemResults.FromError(result.Error!);
            }

            // Rotate: set the new refresh cookie.
            RefreshTokenCookieHelper.SetRefreshTokenCookie(
                response: httpContext.Response,
                rawRefreshToken: result.Value!.RefreshToken,
                expiresAtUtc: result.Value.RefreshTokenExpiresAt,
                options: options);

            return ApiResults.Ok(ToRefreshResponse(result.Value!));
        }).RequireRateLimiting(AuthRateLimitPolicies.Refresh);

        // ----- Logout (cookie + CSRF) -----
        group.MapPost("/logout", async (
            IMediator mediator,
            HttpContext httpContext,
            IOptions<AuthCookieOptions> cookieOptions,
            IOptions<CsrfOptions> csrfOptions,
            CancellationToken ct) =>
        {
            var options = cookieOptions.Value;
            var token = httpContext.Request.Cookies[options.RefreshTokenCookieName];

            if (string.IsNullOrWhiteSpace(token))
            {
                // Idempotent: nothing to revoke.
                return ApiResults.Empty();
            }

            var csrfError = ValidateCsrf(httpContext, csrfOptions.Value);
            if (csrfError is not null)
            {
                return csrfError;
            }

            var command = new Application.Commands.Logout.LogoutCommand(token);
            var result = await mediator.Send(command, ct);

            RefreshTokenCookieHelper.ClearRefreshTokenCookie(httpContext.Response, options);

            return result.IsSuccess ? ApiResults.Empty() : AuthProblemResults.FromError(result.Error!);
        }).RequireAuthorization().RequireRateLimiting(AuthRateLimitPolicies.Logout);

        // ----- Logout-all (CSRF) -----
        group.MapPost("/logout-all", async (
            IMediator mediator,
            ICurrentUser currentUser,
            HttpContext httpContext,
            IOptions<AuthCookieOptions> cookieOptions,
            IOptions<CsrfOptions> csrfOptions,
            CancellationToken ct) =>
        {
            var userId = currentUser.UserId;
            if (!userId.HasValue)
            {
                return AuthProblemResults.Unauthorized();
            }

            var csrfError = ValidateCsrf(httpContext, csrfOptions.Value);
            if (csrfError is not null)
            {
                return csrfError;
            }

            var command = new LogoutAllCommand(userId.Value);
            var result = await mediator.Send(command, ct);
            if (!result.IsSuccess)
            {
                return AuthProblemResults.FromError(result.Error!);
            }

            RefreshTokenCookieHelper.ClearRefreshTokenCookie(httpContext.Response, cookieOptions.Value);

            return ApiResults.Empty();
        }).RequireAuthorization().RequireRateLimiting(AuthRateLimitPolicies.Logout);

        // ----- Sessions list (safe GET, no CSRF) -----
        group.MapGet("/sessions", async (
            IMediator mediator,
            ICurrentUser currentUser,
            int? page,
            int? pageSize,
            CancellationToken ct) =>
        {
            var userId = currentUser.UserId;
            if (!userId.HasValue)
            {
                return AuthProblemResults.Unauthorized();
            }

            var query = new GetUserSessionsQuery(
                UserId: userId.Value,
                Page: page ?? 1,
                PageSize: pageSize ?? 20);
            var result = await mediator.Send(query, ct);
            if (!result.IsSuccess)
            {
                return AuthProblemResults.FromError(result.Error!);
            }

            var resultPage = result.Value!;
            var response = resultPage.Items.Select(ToUserSessionResponse).ToList();
            return ApiResults.Paged(
                items: response,
                page: resultPage.Page,
                pageSize: resultPage.PageSize,
                totalCount: resultPage.TotalCount);
        }).RequireAuthorization();

        // ----- Session revoke (CSRF) -----
        group.MapDelete("/sessions/{sessionId:guid}", async (
            Guid sessionId,
            IMediator mediator,
            ICurrentUser currentUser,
            HttpContext httpContext,
            IOptions<CsrfOptions> csrfOptions,
            CancellationToken ct) =>
        {
            var userId = currentUser.UserId;
            if (!userId.HasValue)
            {
                return AuthProblemResults.Unauthorized();
            }

            var csrfError = ValidateCsrf(httpContext, csrfOptions.Value);
            if (csrfError is not null)
            {
                return csrfError;
            }

            bool isAdmin =
                currentUser.IsInRole(IdentityEndpointAuthorization.AdminRole) ||
                currentUser.IsInRole(IdentityEndpointAuthorization.OwnerRole);
            var command = new RevokeSessionCommand(sessionId, userId.Value, isAdmin);
            var result = await mediator.Send(command, ct);

            return result.IsSuccess ? ApiResults.Empty() : AuthProblemResults.FromError(result.Error!);
        }).RequireAuthorization().RequireRateLimiting(AuthRateLimitPolicies.SessionMutation);

        return routes;
    }

    // Real double-submit CSRF validation. Returns null when valid; otherwise a
    // stable ProblemDetails result. Header presence alone is NOT accepted (H-03).
    private static IResult? ValidateCsrf(HttpContext httpContext, CsrfOptions options) =>
        CsrfProtection.Validate(httpContext.Request, options) switch
        {
            CsrfValidationResult.Missing => AuthProblemResults.CsrfMissing(),
            CsrfValidationResult.Invalid => AuthProblemResults.CsrfInvalid(),
            _ => null
        };

    // ----- Application result -> Presentation DTO mapping (HTTP boundary) -----
    private static LoginResponse ToLoginResponse(LoginResult r) =>
        new(r.AccessToken, r.AccessTokenExpiresAt);

    private static RefreshResponse ToRefreshResponse(RefreshResult r) =>
        new(r.AccessToken, r.AccessTokenExpiresAt);

    private static UserSessionResponse ToUserSessionResponse(UserSessionResult s) =>
        new(s.Id, s.DeviceId, s.DeviceName, s.UserAgentHash, s.IpAddressHash, s.CreatedAtUtc, s.LastSeenAtUtc);
}
