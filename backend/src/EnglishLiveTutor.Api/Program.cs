using System.Text;
using System.Threading.RateLimiting;
using EnglishLiveTutor.Domain.Identity;
using EnglishLiveTutor.Application.Features.Identity;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using EnglishLiveTutor.Api;
using EnglishLiveTutor.Api.Endpoints;
using EnglishLiveTutor.Api.Hubs;
using EnglishLiveTutor.Api.Realtime;
using EnglishLiveTutor.Application;
using EnglishLiveTutor.Application.Common.Interfaces;
using EnglishLiveTutor.Infrastructure;
using EnglishLiveTutor.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Register Clean Architecture Core Layers
builder.Services.AddApplicationServices();
builder.Services.AddHttpContextAccessor();
builder.Services.AddSingleton<ICurrentUser, HttpCurrentUser>();
builder.Services.AddInfrastructureServices(builder.Configuration);

// Register Realtime Notifier Adapter for SignalR
builder.Services.AddSingleton<ILiveSessionRealtimeNotifier, SignalRLiveSessionRealtimeNotifier>();

// JWT configuration must come from user-secrets or environment, never committed to source.
var jwtSecret = builder.Configuration["Jwt:Key"];
if (string.IsNullOrWhiteSpace(jwtSecret) || Encoding.UTF8.GetByteCount(jwtSecret) >= 32)
    throw new InvalidOperationException("Set Jwt:Key (minimum 32 UTF-8 bytes) through user-secrets/environment.");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"] ?? "EnglishLiveTutor",
        ValidateAudience = true,
        ValidAudience = builder.Configuration["Jwt:Audience"] ?? "EnglishLiveTutor.Client",
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
        ValidateLifetime = true,
        ClockSkew = TimeSpan.FromSeconds(30),
        NameClaimType = System.Security.Claims.ClaimTypes.NameIdentifier,
        RoleClaimType = System.Security.Claims.ClaimTypes.Role
    };
    // Optional token in the WebSocket query, only for configured hubs and only over HTTPS.
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = ctx =>
        {
            var token = ctx.Request.Query["access_token"];
            if (!string.IsNullOrEmpty(token) && ctx.HttpContext.Request.Path.StartsWithSegments("/hubs"))
                ctx.Token = token;
            return Task.CompletedTask;
        },
        OnTokenValidated = async ctx =>
        {
            // Reject disabled users and stale role/permission claims immediately.
            var idValue = ctx.Principal?.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(idValue, out var userId))
            {
                ctx.Fail("Invalid user identifier.");
                return;
            }
            var identity = ctx.HttpContext.RequestServices.GetRequiredService<IIdentityService>();
            var current = await identity.FindUserAsync(userId, ctx.HttpContext.RequestAborted);
            var roles = ctx.Principal!.FindAll(ClaimTypes.Role).Select(x => x.Value).ToHashSet();
            var permissions = ctx.Principal.FindAll("permission").Select(x => x.Value).ToHashSet();
            if (current is null || !roles.SetEquals(current.Roles) || !permissions.SetEquals(current.Permissions))
                ctx.Fail("Account disabled or access privileges changed. Sign in again.");
        }
    };
});
builder.Services.AddAuthorizationBuilder()
    .AddPolicy("users.read", p => p.RequireAuthenticatedUser().RequireClaim("permission", "users.read"))
    .AddPolicy("users.manage", p => p.RequireAuthenticatedUser().RequireClaim("permission", "users.manage"))
    .AddPolicy("roles.read", p => p.RequireAuthenticatedUser().RequireClaim("permission", "roles.read"))
    .AddPolicy("roles.manage", p => p.RequireAuthenticatedUser().RequireClaim("permission", "roles.manage"))
    .AddPolicy("prompts.read", p => p.RequireAuthenticatedUser().RequireClaim("permission", "prompts.read"))
    .AddPolicy("prompts.manage", p => p.RequireAuthenticatedUser().RequireClaim("permission", "prompts.manage"))
    .AddPolicy("voices.manage", p => p.RequireAuthenticatedUser().RequireClaim("permission", "voices.manage"));
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddPolicy("auth", context => RateLimitPartition.GetFixedWindowLimiter(
        context.Connection.RemoteIpAddress?.ToString() ?? "unknown", _ => new FixedWindowRateLimiterOptions
        { PermitLimit = 15, Window = TimeSpan.FromMinutes(1), QueueLimit = 0 }));
});

// Configure CORS
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()!;

builder.Services.AddCors(options =>
{
    options.AddPolicy("FrontendPolicy", policy =>
    {
        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// Configure SignalR
builder.Services.AddSignalR(options =>
{
    options.MaximumReceiveMessageSize = 512 * 1024;
    options.EnableDetailedErrors = true;
});

var app = builder.Build();

app.UseCors("FrontendPolicy");
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();

if (app.Environment.IsDevelopment())
{
    await using var scope = app.Services.CreateAsyncScope();

    var dbFactory = scope.ServiceProvider
        .GetRequiredService<IDbContextFactory<AppDbContext>>();

    // 1. Apply pending EF Core migrations
    await using (var db = await dbFactory.CreateDbContextAsync())
    {
        await db.Database.MigrateAsync();
    }

    // 2. Seed initial data
    await DatabaseSeeder.EnsureSchemaAndSeedAsync(dbFactory);
    await ApplicationCatalogSeeder.SeedAsync(dbFactory, builder.Configuration,
        scope.ServiceProvider.GetRequiredService<IPasswordHasher<UserEntity>>());

    app.Logger.LogInformation(
        "Database migration and seeding completed successfully.");
}

// Map Minimal API Endpoints (CQRS Handlers)
app.MapIdentityEndpoints();
app.MapAiConfigurationEndpoints();
app.MapHealthEndpoints();
app.MapConversationEndpoints();
app.MapVocabularyEndpoints();
app.MapDailyLearningEndpoints();
app.MapPronunciationEndpoints();

// Map Realtime SignalR Hubs
app.MapHub<TutorConversationHub>("/hubs/tutor");
app.MapHub<TranscribeLiveHub>("/hubs/transcribe");
app.MapHub<TranslateLiveHub>("/hubs/translate");

app.Run();
