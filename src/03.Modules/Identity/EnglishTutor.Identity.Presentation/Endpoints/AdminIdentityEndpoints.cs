using EnglishTutor.BuildingBlocks.Application.CurrentUser;
using EnglishTutor.BuildingBlocks.Presentation.Responses;
using EnglishTutor.Identity.Application.Users.Commands.CreateRole;
using EnglishTutor.Identity.Application.Users.Commands.CreateUser;
using EnglishTutor.Identity.Application.Users.Commands.SetRolePermissions;
using EnglishTutor.Identity.Application.Users.Commands.SetUserActive;
using EnglishTutor.Identity.Application.Users.Commands.SetUserRoles;
using EnglishTutor.Identity.Application.Users.Commands.UpdateRole;
using EnglishTutor.Identity.Application.Users.Commands.UpdateUser;
using EnglishTutor.Identity.Application.Users.Queries.GetUser;
using EnglishTutor.Identity.Application.Users.Queries.ListPermissions;
using EnglishTutor.Identity.Application.Users.Queries.ListRoles;
using EnglishTutor.Identity.Application.Users.Queries.ListUsers;
using EnglishTutor.Identity.Presentation.Endpoints.Dtos;
using EnglishTutor.Identity.Presentation.RateLimiting;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace EnglishTutor.Identity.Presentation.Endpoints;

public static class AdminIdentityEndpoints
{
    public static IEndpointRouteBuilder MapAdminIdentityEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes
            .MapGroup("/api/admin/identity")
            .WithTags("Identity Admin")
            .RequireAuthorization();

        group.MapGet("/users", async (
            IMediator mediator,
            ICurrentUser currentUser,
            int? page,
            int? pageSize,
            string? search,
            string? status,
            string? role,
            CancellationToken ct) =>
        {
            if (!CanManageUsers(currentUser))
            {
                return ApiResults.Problem(
                    statusCode: StatusCodes.Status403Forbidden,
                    code: ApiErrorCodes.Forbidden,
                    title: "Forbidden",
                    message: "Only Owner or Admin users can view users.");
            }

            var result = await mediator.Send(new ListUsersQuery(
                Page: page ?? 1,
                PageSize: pageSize ?? 20,
                Search: search,
                Status: status,
                Role: role), ct);
            if (!result.IsSuccess)
            {
                return ApiResults.FromError(result.Error!);
            }

            var resultPage = result.Value!;
            var response = resultPage.Items.Select(ToUserManagementResponse).ToArray();
            return ApiResults.Paged(
                items: response,
                page: resultPage.Page,
                pageSize: resultPage.PageSize,
                totalCount: resultPage.TotalCount);
        });

        group.MapGet("/users/{userId:guid}", async (
            Guid userId,
            IMediator mediator,
            ICurrentUser currentUser,
            CancellationToken ct) =>
        {
            if (!CanManageUsers(currentUser))
            {
                return ForbiddenManageUsers();
            }

            var result = await mediator.Send(new GetUserQuery(userId), ct);
            return result.IsSuccess
                ? ApiResults.Ok(ToUserDetailResponse(result.Value!))
                : ApiResults.FromError(result.Error!);
        });

        group.MapPost("/users", async (
            CreateUserRequest request,
            IMediator mediator,
            ICurrentUser currentUser,
            CancellationToken ct) =>
        {
            if (!CanManageUsers(currentUser))
            {
                return ForbiddenManageUsers();
            }

            var command = new CreateUserCommand(
                Email: request.Email,
                Password: request.Password,
                DisplayName: request.DisplayName,
                Roles: request.Roles ?? [],
                IsActive: request.IsActive ?? true,
                CreatedByUserId: currentUser.UserId,
                ActorIsOwner: IsOwner(currentUser));
            var result = await mediator.Send(command, ct);

            return result.IsSuccess
                ? ApiResults.Created($"/api/admin/identity/users/{result.Value!.Id}", ToUserDetailResponse(result.Value))
                : ApiResults.FromError(result.Error!);
        }).RequireRateLimiting(AuthRateLimitPolicies.SessionMutation);

        group.MapPut("/users/{userId:guid}", async (
            Guid userId,
            UpdateUserRequest request,
            IMediator mediator,
            ICurrentUser currentUser,
            CancellationToken ct) =>
        {
            if (!CanManageUsers(currentUser))
            {
                return ForbiddenManageUsers();
            }

            var result = await mediator.Send(new UpdateUserCommand(
                UserId: userId,
                DisplayName: request.DisplayName,
                IsActive: request.IsActive,
                ActorIsOwner: IsOwner(currentUser)), ct);

            return result.IsSuccess
                ? ApiResults.Ok(ToUserDetailResponse(result.Value!))
                : ApiResults.FromError(result.Error!);
        }).RequireRateLimiting(AuthRateLimitPolicies.SessionMutation);

        group.MapPut("/users/{userId:guid}/roles", async (
            Guid userId,
            SetUserRolesRequest request,
            IMediator mediator,
            ICurrentUser currentUser,
            CancellationToken ct) =>
        {
            if (!CanManageUsers(currentUser))
            {
                return ForbiddenManageUsers();
            }

            var result = await mediator.Send(new SetUserRolesCommand(
                UserId: userId,
                Roles: request.Roles,
                ActorIsOwner: IsOwner(currentUser)), ct);

            return result.IsSuccess
                ? ApiResults.Ok(ToUserDetailResponse(result.Value!))
                : ApiResults.FromError(result.Error!);
        }).RequireRateLimiting(AuthRateLimitPolicies.SessionMutation);

        group.MapPost("/users/{userId:guid}/activate", async (
            Guid userId,
            IMediator mediator,
            ICurrentUser currentUser,
            CancellationToken ct) =>
        {
            if (!CanManageUsers(currentUser))
            {
                return ForbiddenManageUsers();
            }

            var result = await mediator.Send(new SetUserActiveCommand(
                UserId: userId,
                IsActive: true,
                ActorIsOwner: IsOwner(currentUser)), ct);

            return result.IsSuccess ? ApiResults.Empty() : ApiResults.FromError(result.Error!);
        }).RequireRateLimiting(AuthRateLimitPolicies.SessionMutation);

        group.MapPost("/users/{userId:guid}/deactivate", async (
            Guid userId,
            IMediator mediator,
            ICurrentUser currentUser,
            CancellationToken ct) =>
        {
            if (!CanManageUsers(currentUser))
            {
                return ForbiddenManageUsers();
            }

            var result = await mediator.Send(new SetUserActiveCommand(
                UserId: userId,
                IsActive: false,
                ActorIsOwner: IsOwner(currentUser)), ct);

            return result.IsSuccess ? ApiResults.Empty() : ApiResults.FromError(result.Error!);
        }).RequireRateLimiting(AuthRateLimitPolicies.SessionMutation);

        group.MapGet("/roles", async (
            IMediator mediator,
            ICurrentUser currentUser,
            CancellationToken ct) =>
        {
            if (!CanManageUsers(currentUser))
            {
                return ForbiddenManageUsers();
            }

            var result = await mediator.Send(new ListRolesQuery(), ct);
            return result.IsSuccess
                ? ApiResults.Ok(result.Value!.Select(ToRoleResponse).ToArray())
                : ApiResults.FromError(result.Error!);
        });

        group.MapGet("/permissions", async (
            IMediator mediator,
            ICurrentUser currentUser,
            CancellationToken ct) =>
        {
            if (!CanManageUsers(currentUser))
            {
                return ForbiddenManageUsers();
            }

            var result = await mediator.Send(new ListPermissionsQuery(), ct);
            return result.IsSuccess
                ? ApiResults.Ok(result.Value!.Select(ToPermissionResponse).ToArray())
                : ApiResults.FromError(result.Error!);
        });

        group.MapPost("/roles", async (
            CreateRoleRequest request,
            IMediator mediator,
            ICurrentUser currentUser,
            CancellationToken ct) =>
        {
            if (!CanManageRoles(currentUser))
            {
                return ForbiddenManageRoles();
            }

            var result = await mediator.Send(new CreateRoleCommand(
                Name: request.Name,
                DisplayName: request.DisplayName,
                Priority: request.Priority,
                PermissionCodes: request.PermissionCodes ?? []), ct);

            return result.IsSuccess
                ? ApiResults.Created($"/api/admin/identity/roles/{result.Value!.Id}", ToRoleResponse(result.Value))
                : ApiResults.FromError(result.Error!);
        }).RequireRateLimiting(AuthRateLimitPolicies.SessionMutation);

        group.MapPut("/roles/{roleId:guid}", async (
            Guid roleId,
            UpdateRoleRequest request,
            IMediator mediator,
            ICurrentUser currentUser,
            CancellationToken ct) =>
        {
            if (!CanManageRoles(currentUser))
            {
                return ForbiddenManageRoles();
            }

            var result = await mediator.Send(new UpdateRoleCommand(
                RoleId: roleId,
                DisplayName: request.DisplayName,
                Priority: request.Priority), ct);

            return result.IsSuccess
                ? ApiResults.Ok(ToRoleResponse(result.Value!))
                : ApiResults.FromError(result.Error!);
        }).RequireRateLimiting(AuthRateLimitPolicies.SessionMutation);

        group.MapPut("/roles/{roleId:guid}/permissions", async (
            Guid roleId,
            SetRolePermissionsRequest request,
            IMediator mediator,
            ICurrentUser currentUser,
            CancellationToken ct) =>
        {
            if (!CanManageRoles(currentUser))
            {
                return ForbiddenManageRoles();
            }

            var result = await mediator.Send(new SetRolePermissionsCommand(
                RoleId: roleId,
                PermissionCodes: request.PermissionCodes), ct);

            return result.IsSuccess
                ? ApiResults.Ok(ToRoleResponse(result.Value!))
                : ApiResults.FromError(result.Error!);
        }).RequireRateLimiting(AuthRateLimitPolicies.SessionMutation);

        return routes;
    }

    private static bool CanManageUsers(ICurrentUser currentUser) =>
        currentUser.IsInRole(IdentityEndpointAuthorization.OwnerRole) ||
        currentUser.IsInRole(IdentityEndpointAuthorization.AdminRole);

    private static bool IsOwner(ICurrentUser currentUser) =>
        currentUser.IsInRole(IdentityEndpointAuthorization.OwnerRole);

    private static bool CanManageRoles(ICurrentUser currentUser) =>
        IsOwner(currentUser);

    private static IResult ForbiddenManageUsers() =>
        ApiResults.Problem(
            statusCode: StatusCodes.Status403Forbidden,
            code: ApiErrorCodes.Forbidden,
            title: "Forbidden",
            message: "Only Owner or Admin users can manage users.");

    private static IResult ForbiddenManageRoles() =>
        ApiResults.Problem(
            statusCode: StatusCodes.Status403Forbidden,
            code: ApiErrorCodes.Forbidden,
            title: "Forbidden",
            message: "Only Owner users can manage roles.");

    private static UserManagementResponse ToUserManagementResponse(UserListItemResult user) =>
        new(
            Id: user.Id,
            Email: user.Email,
            DisplayName: user.DisplayName,
            IsActive: user.IsActive,
            IsLockedOut: user.IsLockedOut,
            LockoutEndUtc: user.LockoutEndUtc,
            Roles: user.Roles,
            CreatedAtUtc: user.CreatedAtUtc,
            UpdatedAtUtc: user.UpdatedAtUtc);

    private static UserDetailResponse ToUserDetailResponse(UserDetailResult user) =>
        new(
            Id: user.Id,
            Email: user.Email,
            DisplayName: user.DisplayName,
            IsActive: user.IsActive,
            IsLockedOut: user.IsLockedOut,
            LockoutEndUtc: user.LockoutEndUtc,
            FailedLoginAttempts: user.FailedLoginAttempts,
            Roles: user.Roles,
            CreatedAtUtc: user.CreatedAtUtc,
            UpdatedAtUtc: user.UpdatedAtUtc);

    private static RoleResponse ToRoleResponse(RoleResult role) =>
        new(
            Id: role.Id,
            Name: role.Name,
            DisplayName: role.DisplayName,
            Priority: role.Priority,
            PermissionCodes: role.PermissionCodes);

    private static PermissionResponse ToPermissionResponse(PermissionResult permission) =>
        new(
            Id: permission.Id,
            Code: permission.Code,
            ModuleName: permission.ModuleName,
            DisplayName: permission.DisplayName);
}
