using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using EnglishLiveTutor.Application.Features.Identity;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.RateLimiting;

namespace EnglishLiveTutor.Api.Endpoints;

public static class IdentityEndpoints
{
    public sealed record RegisterRequest(string Email, string DisplayName, string Password);
    public sealed record LoginRequest(string Email, string Password);
    public sealed record RefreshRequest(string RefreshToken);
    public sealed record CreateRoleRequest(string Name, string Description);
    public sealed record CreatePermissionRequest(string Code, string Description);
    public sealed record AdminCreateUserRequest(string Email, string DisplayName, string Password, string? Role);
    public sealed record SetEnabledRequest(bool Enabled);
    public sealed record SetMembershipRequest(bool Assigned);
    public sealed record SetPermissionRequest(bool Granted);

    private static (string Token, DateTimeOffset ExpiresAt) IssueJwt(
        IdentityPrincipal principal, IConfiguration config, TimeProvider clock)
    {
        var secret = config.GetValue<string>("Jwt:Key") 
            ?? throw new InvalidOperationException("Configure Jwt:Key using user-secrets/environment.");
        var now = clock.GetUtcNow(); 
        var expires = now.AddMinutes(config.GetValue<int>("Jwt:ExpiryMinutes"));
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, principal.Id.ToString()),
            new(ClaimTypes.NameIdentifier, principal.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, principal.Email),
            new(ClaimTypes.Name, principal.DisplayName),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString("N"))
        };
        claims.AddRange(principal.Roles.Select(role => new Claim(ClaimTypes.Role, role)));
        claims.AddRange(principal.Permissions.Select(permission => new Claim("permission", permission)));
        var jwt = new JwtSecurityToken(
            issuer: config.GetValue<string>("Jwt:Issuer") ?? "EnglishLiveTutor",
            audience: config.GetValue<string>("Jwt:Audience") ?? "EnglishLiveTutor.Client",
            claims: claims, 
            notBefore: now.UtcDateTime, 
            expires: expires.UtcDateTime,
            signingCredentials: new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret)), SecurityAlgorithms.HmacSha256));
        return (new JwtSecurityTokenHandler().WriteToken(jwt), expires);
    }

    public static IEndpointRouteBuilder MapIdentityEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/auth").WithTags("Authentication");

        group.MapPost("/register", async (RegisterRequest request, IIdentityService identity,
            IConfiguration config, TimeProvider clock, CancellationToken ct) =>
        {
            try
            {
                var user = await identity.RegisterAsync(request.Email, request.DisplayName, request.Password, ct);
                if (user is null) return Results.Conflict(new { error = "Account already exists." });
                var (refresh, refreshExpires) = await identity.CreateRefreshTokenAsync(user.Id, ct);
                var (jwt, expires) = IssueJwt(user, config, clock);
                return Results.Ok(new AuthResult(user, new TokenPair(jwt, refresh, expires, refreshExpires)));
            }
            catch (ArgumentException e) { return Results.BadRequest(new { error = e.Message }); }
        }).RequireRateLimiting("auth");

        group.MapPost("/login", async (LoginRequest request, IIdentityService identity,
            IConfiguration config, TimeProvider clock, CancellationToken ct) =>
        {
            var user = await identity.VerifyCredentialsAsync(request.Email, request.Password, ct);
            if (user is null) return Results.Unauthorized();
            var (refresh, refreshExpires) = await identity.CreateRefreshTokenAsync(user.Id, ct);
            var (jwt, expires) = IssueJwt(user, config, clock);
            return Results.Ok(new AuthResult(user, new TokenPair(jwt, refresh, expires, refreshExpires)));
        }).RequireRateLimiting("auth");

        group.MapPost("/refresh", async (RefreshRequest request, IIdentityService identity,
            IConfiguration config, TimeProvider clock, CancellationToken ct) =>
        {
            var rotated = await identity.RotateRefreshTokenAsync(request.RefreshToken, ct);
            if (rotated is null) return Results.Unauthorized();
            var (jwt, expires) = IssueJwt(rotated.Value.User, config, clock);
            return Results.Ok(new AuthResult(rotated.Value.User,
                new TokenPair(jwt, rotated.Value.RefreshToken, expires, rotated.Value.ExpiresAt)));
        }).RequireRateLimiting("auth");

        group.MapPost("/logout", async (RefreshRequest request, IIdentityService identity, CancellationToken ct) =>
        {
            await identity.RevokeRefreshTokenAsync(request.RefreshToken, null, ct);
            return Results.NoContent();
        }).RequireRateLimiting("auth");

        group.MapGet("/me", async (ClaimsPrincipal actor, IIdentityService identity, CancellationToken ct) =>
        {
            if (!Guid.TryParse(actor.FindFirstValue(ClaimTypes.NameIdentifier), out var id))
                return Results.Unauthorized();
            var user = await identity.FindUserAsync(id, ct);
            return user is null ? Results.Unauthorized() : Results.Ok(user);
        }).RequireAuthorization();

        var admin = app.MapGroup("/api/admin").WithTags("Administration").RequireAuthorization();
        admin.MapPost("/users", async (AdminCreateUserRequest request, IIdentityService identity,
            CancellationToken ct) =>
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(request.Role) &&
                    !(await identity.ListRolesAsync(ct)).Any(r => r.Name == request.Role))
                    return Results.BadRequest(new { error = "Unknown role." });
                var user = await identity.RegisterAsync(request.Email, request.DisplayName, request.Password, ct);
                if (user is null) return Results.Conflict(new { error = "Account already exists." });
                if (!string.IsNullOrWhiteSpace(request.Role) && request.Role != "Student")
                    await identity.SetUserRoleAsync(user.Id, request.Role, true, ct);
                return Results.Created($"/api/admin/users/{user.Id}", user);
            }
            catch (ArgumentException ex) { return Results.BadRequest(new { error = ex.Message }); }
        }).RequireAuthorization("users.manage");
        admin.MapGet("/users", async (IIdentityService identity, CancellationToken ct) =>
            Results.Ok(await identity.ListUsersAsync(ct))).RequireAuthorization("users.read");
        admin.MapPut("/users/{id:guid}/enabled", async (Guid id, SetEnabledRequest request,
            ClaimsPrincipal actor, IIdentityService identity, CancellationToken ct) =>
        {
            if (!request.Enabled && actor.FindFirstValue(ClaimTypes.NameIdentifier) == id.ToString())
                return Results.BadRequest(new { error = "Cannot disable your own account." });
            try { return await identity.SetUserEnabledAsync(id, request.Enabled, ct) ? Results.NoContent() : Results.NotFound(); }
            catch (InvalidOperationException ex) { return Results.BadRequest(new { error = ex.Message }); }
        }).RequireAuthorization("users.manage");
        admin.MapPut("/users/{id:guid}/roles/{role}", async (Guid id, string role,
            SetMembershipRequest request, ClaimsPrincipal actor, IIdentityService identity, CancellationToken ct) =>
        {
            if (!request.Assigned && role == "Admin" && actor.FindFirstValue(ClaimTypes.NameIdentifier) == id.ToString())
                return Results.BadRequest(new { error = "Cannot remove your own Admin role." });
            try { return await identity.SetUserRoleAsync(id, role, request.Assigned, ct) ? Results.NoContent() : Results.NotFound(); }
            catch (InvalidOperationException ex) { return Results.BadRequest(new { error = ex.Message }); }
        }).RequireAuthorization("users.manage");
        admin.MapPost("/roles", async (CreateRoleRequest request, IIdentityService identity, CancellationToken ct) =>
        {
            try { return await identity.CreateRoleAsync(request.Name, request.Description, ct)
                ? Results.Created($"/api/admin/roles/{request.Name}", new { request.Name }) : Results.Conflict(); }
            catch (ArgumentException ex) { return Results.BadRequest(new { error = ex.Message }); }
        }).RequireAuthorization("roles.manage");
        admin.MapDelete("/roles/{name}", async (string name, IIdentityService identity, CancellationToken ct) =>
        {
            try { return await identity.DeleteRoleAsync(name, ct) ? Results.NoContent() : Results.NotFound(); }
            catch (InvalidOperationException ex) { return Results.BadRequest(new { error = ex.Message }); }
        }).RequireAuthorization("roles.manage");
        admin.MapPost("/permissions", async (CreatePermissionRequest request, IIdentityService identity, CancellationToken ct) =>
        {
            try { return await identity.CreatePermissionAsync(request.Code, request.Description, ct)
                ? Results.Created($"/api/admin/permissions/{request.Code}", new { request.Code }) : Results.Conflict(); }
            catch (ArgumentException ex) { return Results.BadRequest(new { error = ex.Message }); }
        }).RequireAuthorization("roles.manage");
        admin.MapDelete("/permissions/{code}", async (string code, IIdentityService identity, CancellationToken ct) =>
        {
            try { return await identity.DeletePermissionAsync(code, ct) ? Results.NoContent() : Results.NotFound(); }
            catch (InvalidOperationException ex) { return Results.BadRequest(new { error = ex.Message }); }
        }).RequireAuthorization("roles.manage");
        admin.MapGet("/roles", async (IIdentityService identity, CancellationToken ct) =>
            Results.Ok(await identity.ListRolesAsync(ct))).RequireAuthorization("roles.read");
        admin.MapGet("/permissions", async (IIdentityService identity, CancellationToken ct) =>
            Results.Ok(await identity.ListPermissionsAsync(ct))).RequireAuthorization("roles.read");
        admin.MapPut("/roles/{role}/permissions/{code}", async (string role, string code,
            SetPermissionRequest request, IIdentityService identity, CancellationToken ct) =>
        {
            try { return await identity.SetRolePermissionAsync(role, code, request.Granted, ct)
                ? Results.NoContent() : Results.NotFound(); }
            catch (InvalidOperationException ex) { return Results.BadRequest(new { error = ex.Message }); }
        }).RequireAuthorization("roles.manage");
        return app;
    }
}
