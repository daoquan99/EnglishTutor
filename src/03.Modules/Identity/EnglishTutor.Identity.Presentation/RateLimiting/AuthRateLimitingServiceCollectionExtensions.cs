using System;
using System.Threading.RateLimiting;
using EnglishTutor.Identity.Presentation.Auth;
using EnglishTutor.BuildingBlocks.Presentation.Responses;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EnglishTutor.Identity.Presentation.RateLimiting;

/// <summary>
/// Registers the in-process auth rate-limiter and its named policies, and a
/// stable <c>429</c> ProblemDetails rejection response with <c>Retry-After</c>
/// (Batch R1, H-06).
/// </summary>
public static class AuthRateLimitingServiceCollectionExtensions
{
    public static IServiceCollection AddIdentityRateLimiting(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services
            .AddOptions<AuthRateLimitOptions>()
            .Bind(configuration.GetSection(AuthRateLimitOptions.SectionName))
            .Validate(options => IsValid(options.Login), "Auth:RateLimit:Login must have PermitLimit 1-1000 and WindowSeconds 1-3600.")
            .Validate(options => IsValid(options.Refresh), "Auth:RateLimit:Refresh must have PermitLimit 1-1000 and WindowSeconds 1-3600.")
            .Validate(options => IsValid(options.Logout), "Auth:RateLimit:Logout must have PermitLimit 1-1000 and WindowSeconds 1-3600.")
            .Validate(options => IsValid(options.SessionMutation), "Auth:RateLimit:SessionMutation must have PermitLimit 1-1000 and WindowSeconds 1-3600.")
            .ValidateOnStart();

        var options = configuration.GetSection(AuthRateLimitOptions.SectionName)
            .Get<AuthRateLimitOptions>() ?? new AuthRateLimitOptions();

        services.AddRateLimiter(limiter =>
        {
            limiter.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            AddPolicy(limiter, AuthRateLimitPolicies.Login, options, options.Login, ClientKey);
            AddPolicy(limiter, AuthRateLimitPolicies.Refresh, options, options.Refresh, ClientKey);
            AddPolicy(limiter, AuthRateLimitPolicies.Logout, options, options.Logout, UserOrClientKey);
            AddPolicy(limiter, AuthRateLimitPolicies.SessionMutation, options, options.SessionMutation, UserOrClientKey);

            limiter.OnRejected = async (context, token) =>
            {
                var response = context.HttpContext.Response;
                response.StatusCode = StatusCodes.Status429TooManyRequests;

                if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
                {
                    response.Headers.RetryAfter =
                        ((int)retryAfter.TotalSeconds).ToString(System.Globalization.CultureInfo.InvariantCulture);
                }

                await ApiResults.Problem(
                        statusCode: StatusCodes.Status429TooManyRequests,
                        code: AuthErrorCodes.RateLimited,
                        title: "Too many requests",
                        message: "Rate limit exceeded. Please retry later.")
                    .ExecuteAsync(context.HttpContext);
            };
        });

        return services;
    }

    private static void AddPolicy(
        RateLimiterOptions limiter,
        string policyName,
        AuthRateLimitOptions options,
        AuthRateLimitOptions.PolicyLimit limit,
        Func<HttpContext, string> keySelector)
    {
        limiter.AddPolicy(policyName, httpContext =>
        {
            if (!options.Enabled)
            {
                return RateLimitPartition.GetNoLimiter("disabled");
            }

            return RateLimitPartition.GetFixedWindowLimiter(
                keySelector(httpContext),
                _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = limit.PermitLimit,
                    Window = TimeSpan.FromSeconds(limit.WindowSeconds),
                    QueueLimit = 0,
                    AutoReplenishment = true
                });
        });
    }

    private static bool IsValid(AuthRateLimitOptions.PolicyLimit limit) =>
        limit.PermitLimit is >= 1 and <= 1000
        && limit.WindowSeconds is >= 1 and <= 3600;

    // Anonymous endpoints (login/refresh) key on the client IP. Email-composite
    // keying is intentionally avoided here because the request body is not
    // available at the limiter partition stage; per-account brute-force is
    // additionally covered by domain lockout (5 attempts / 15 min).
    private static string ClientKey(HttpContext ctx) =>
        ctx.Connection.RemoteIpAddress?.ToString() ?? "unknown";

    // Authenticated mutations key on the user id, falling back to client IP.
    private static string UserOrClientKey(HttpContext ctx)
    {
        var sub = ctx.User?.FindFirst("sub")?.Value
            ?? ctx.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        return string.IsNullOrEmpty(sub) ? ClientKey(ctx) : sub;
    }
}
