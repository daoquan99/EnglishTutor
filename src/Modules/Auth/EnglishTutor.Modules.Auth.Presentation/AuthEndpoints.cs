using EnglishTutor.BuildingBlocks.Application.Abstractions;
using EnglishTutor.BuildingBlocks.Presentation;
using EnglishTutor.Modules.Auth.Application.Shared.DTOs;
using EnglishTutor.Modules.Auth.Application.Shared.Errors;
using EnglishTutor.BuildingBlocks.Application.Results;
using EnglishTutor.Modules.Auth.Application.Commands.CreatePermission;
using EnglishTutor.Modules.Auth.Application.Commands.CreateRole;
using EnglishTutor.Modules.Auth.Application.Commands.DeletePermission;
using EnglishTutor.Modules.Auth.Application.Commands.DeleteRole;
using EnglishTutor.Modules.Auth.Application.Commands.Login;
using EnglishTutor.Modules.Auth.Application.Commands.Logout;
using EnglishTutor.Modules.Auth.Application.Commands.RefreshToken;
using EnglishTutor.Modules.Auth.Application.Commands.Register;
using EnglishTutor.Modules.Auth.Application.Commands.UpdatePermission;
using EnglishTutor.Modules.Auth.Application.Commands.UpdateRole;
using EnglishTutor.Modules.Auth.Application.Queries.GetCurrentUser;
using EnglishTutor.Modules.Auth.Application.Queries.GetPermission;
using EnglishTutor.Modules.Auth.Application.Queries.GetRole;
using EnglishTutor.Modules.Auth.Application.Queries.ListPermissions;
using EnglishTutor.Modules.Auth.Application.Queries.ListRoles;
using EnglishTutor.Modules.Auth.Contracts.Permissions;
using EnglishTutor.Modules.Auth.Presentation.Requests;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Options;

namespace EnglishTutor.Modules.Auth.Presentation;

public static class AuthEndpoints
{
    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/auth").WithTags("Auth");

        group.MapPost("/register", async (RegisterRequest request, HttpContext httpContext, ISender sender, IOptions<AuthCookieSettings> cookieSettings, CancellationToken ct) =>
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
                AuthCookieManager.SetAuthCookies(httpContext.Response, result.Value, cookieSettings.Value);
            }

            return result.ToCreatedResult();
        })
            .AllowAnonymous()
            .RequireRateLimiting("auth-credentials")
            .WithName("Register");

        group.MapPost("/login", async (LoginRequest request, HttpContext httpContext, ISender sender, IOptions<AuthCookieSettings> cookieSettings, CancellationToken ct) =>
        {
            var result = await sender.Send(new LoginCommand(
                request.Email,
                request.Password,
                AuthCookieManager.GetOrCreateDeviceId(httpContext.Request),
                httpContext.Request.Headers.UserAgent.ToString(),
                httpContext.Connection.RemoteIpAddress?.ToString()), ct);

            if (result.IsSuccess)
            {
                AuthCookieManager.SetAuthCookies(httpContext.Response, result.Value, cookieSettings.Value);
            }

            return result.ToHttpResult();
        })
            .AllowAnonymous()
            .RequireRateLimiting("auth-credentials")
            .WithName("Login");

        group.MapPost("/refresh-token", async (HttpContext httpContext, ISender sender, IOptions<AuthCookieSettings> cookieSettings, CancellationToken ct) =>
        {
            if (!AuthCookieManager.TryReadRefreshCookies(httpContext.Request, out var refreshToken, out var sessionId, out var deviceId))
            {
                AuthCookieManager.ClearAuthCookies(httpContext.Response, cookieSettings.Value);
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
                AuthCookieManager.SetAuthCookies(httpContext.Response, result.Value, cookieSettings.Value);
            }
            else
            {
                AuthCookieManager.ClearAuthCookies(httpContext.Response, cookieSettings.Value);
            }

            return result.ToHttpResult();
        })
            .AllowAnonymous()
            .RequireRateLimiting("auth-refresh")
            .WithName("RefreshToken");

        group.MapPost("/logout", async (HttpContext httpContext, ISender sender, IOptions<AuthCookieSettings> cookieSettings, CancellationToken ct) =>
        {
            if (!AuthCookieManager.TryReadRefreshCookies(httpContext.Request, out var refreshToken, out var sessionId, out var deviceId))
            {
                AuthCookieManager.ClearAuthCookies(httpContext.Response, cookieSettings.Value);
                return Result.Failure(AuthErrors.RefreshTokenNotFound).ToHttpResult();
            }

            var result = await sender.Send(new LogoutCommand(refreshToken, sessionId, deviceId), ct);
            AuthCookieManager.ClearAuthCookies(httpContext.Response, cookieSettings.Value);
            return result.ToHttpResult();
        })
            .AllowAnonymous()
            .WithName("Logout");

        group.MapGet("/me", async (ISender sender, CancellationToken ct) =>
            (await sender.Send(new GetCurrentUserQuery(), ct)).ToHttpResult())
            .RequireAuthorization()
            .WithName("GetCurrentUser");

        var adminGroup = endpoints.MapGroup("/api/admin/auth")
            .RequireAuthorization()
            .WithTags("Auth Admin");

        adminGroup.MapGet("/permissions", async (
            bool? includeDisabled,
            ICurrentUser currentUser,
            ISender sender,
            CancellationToken ct) =>
        {
            if (!currentUser.HasPermission(PermissionCodes.AuthPermissionsRead))
            {
                return Results.Forbid();
            }

            return (await sender.Send(new ListAuthPermissionsQuery(includeDisabled ?? false), ct)).ToHttpResult();
        })
            .WithName("ListAuthPermissions");

        adminGroup.MapPost("/permissions", async (
            CreateAuthPermissionRequest request,
            ICurrentUser currentUser,
            ISender sender,
            CancellationToken ct) =>
        {
            if (!currentUser.HasPermission(PermissionCodes.AuthPermissionsManage))
            {
                return Results.Forbid();
            }

            return (await sender.Send(new CreateAuthPermissionCommand(request.Code, request.Description), ct))
                .ToCreatedResult();
        })
            .WithName("CreateAuthPermission");

        adminGroup.MapGet("/permissions/{permissionId:guid}", async (
            Guid permissionId,
            ICurrentUser currentUser,
            ISender sender,
            CancellationToken ct) =>
        {
            if (!currentUser.HasPermission(PermissionCodes.AuthPermissionsRead))
            {
                return Results.Forbid();
            }

            return (await sender.Send(new GetAuthPermissionQuery(permissionId), ct)).ToHttpResult();
        })
            .WithName("GetAuthPermission");

        adminGroup.MapPut("/permissions/{permissionId:guid}", async (
            Guid permissionId,
            UpdateAuthPermissionRequest request,
            ICurrentUser currentUser,
            ISender sender,
            CancellationToken ct) =>
        {
            if (!currentUser.HasPermission(PermissionCodes.AuthPermissionsManage))
            {
                return Results.Forbid();
            }

            return (await sender.Send(new UpdateAuthPermissionCommand(permissionId, request.Description, request.IsEnabled), ct))
                .ToHttpResult();
        })
            .WithName("UpdateAuthPermission");

        adminGroup.MapDelete("/permissions/{permissionId:guid}", async (
            Guid permissionId,
            ICurrentUser currentUser,
            ISender sender,
            CancellationToken ct) =>
        {
            if (!currentUser.HasPermission(PermissionCodes.AuthPermissionsManage))
            {
                return Results.Forbid();
            }

            return (await sender.Send(new DeleteAuthPermissionCommand(permissionId), ct)).ToHttpResult();
        })
            .WithName("DeleteAuthPermission");

        adminGroup.MapGet("/roles", async (
            bool? includeDisabled,
            ICurrentUser currentUser,
            ISender sender,
            CancellationToken ct) =>
        {
            if (!currentUser.HasPermission(PermissionCodes.AuthRolesRead))
            {
                return Results.Forbid();
            }

            return (await sender.Send(new ListAuthRolesQuery(includeDisabled ?? false), ct)).ToHttpResult();
        })
            .WithName("ListAuthRoles");

        adminGroup.MapGet("/roles/{roleId:guid}", async (
            Guid roleId,
            ICurrentUser currentUser,
            ISender sender,
            CancellationToken ct) =>
        {
            if (!currentUser.HasPermission(PermissionCodes.AuthRolesRead))
            {
                return Results.Forbid();
            }

            return (await sender.Send(new GetAuthRoleQuery(roleId), ct)).ToHttpResult();
        })
            .WithName("GetAuthRole");

        adminGroup.MapPost("/roles", async (
            CreateAuthRoleRequest request,
            ICurrentUser currentUser,
            ISender sender,
            CancellationToken ct) =>
        {
            if (!currentUser.HasPermission(PermissionCodes.AuthRolesManage))
            {
                return Results.Forbid();
            }

            return (await sender.Send(new CreateAuthRoleCommand(
                request.Name,
                request.Description,
                request.IsEnabled,
                request.PermissionIds), ct)).ToCreatedResult();
        })
            .WithName("CreateAuthRole");

        adminGroup.MapPut("/roles/{roleId:guid}", async (
            Guid roleId,
            UpdateAuthRoleRequest request,
            ICurrentUser currentUser,
            ISender sender,
            CancellationToken ct) =>
        {
            if (!currentUser.HasPermission(PermissionCodes.AuthRolesManage))
            {
                return Results.Forbid();
            }

            return (await sender.Send(new UpdateAuthRoleCommand(
                roleId,
                request.Name,
                request.Description,
                request.IsEnabled,
                request.PermissionIds), ct)).ToHttpResult();
        })
            .WithName("UpdateAuthRole");

        adminGroup.MapDelete("/roles/{roleId:guid}", async (
            Guid roleId,
            ICurrentUser currentUser,
            ISender sender,
            CancellationToken ct) =>
        {
            if (!currentUser.HasPermission(PermissionCodes.AuthRolesManage))
            {
                return Results.Forbid();
            }

            return (await sender.Send(new DeleteAuthRoleCommand(roleId), ct)).ToHttpResult();
        })
            .WithName("DeleteAuthRole");

        return endpoints;
    }
}
