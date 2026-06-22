using Microsoft.AspNetCore.Http;

namespace EnglishTutor.BuildingBlocks.Infrastructure.Correlation;

/// <summary>
/// Reads, sanitizes, and propagates a correlation id for each HTTP request.
/// </summary>
/// <remarks>
/// <para>Id is taken from the <c>X-Correlation-Id</c> request header if present.</para>
/// <para>Sanitization prevents log injection, header smuggling, and unbounded
/// growth: input is length-capped at <see cref="MaxLength"/> characters and
/// only contains characters from <see cref="AllowedCharacters"/>.</para>
/// <para>Fallback chain when the header is missing or invalid:</para>
/// <list type="number">
///   <item><c>context.TraceIdentifier</c> (ASP.NET Core assigns a hex GUID per request).</item>
///   <item>A freshly generated GUID (only if TraceIdentifier is also missing).</item>
/// </list>
/// </remarks>
public sealed class CorrelationIdMiddleware
{
    public const string HeaderName = "X-Correlation-Id";
    public const string HttpContextItemKey = "CorrelationId";
    public const int MaxLength = 64;

    /// <summary>
    /// Characters allowed in an inbound correlation id. Includes letters,
    /// digits, <c>-</c>, <c>_</c>, <c>.</c>, and <c>+</c> (for base64-encoded
    /// trace ids used by some tracing systems). Excludes <c>:</c> because it
    /// collides with structured-log key/value separators.
    /// </summary>
    private const string AllowedCharacters =
        "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-_.";

    private static readonly bool[] AllowedCharLookup = BuildLookup(AllowedCharacters);

    private readonly RequestDelegate _next;

    public CorrelationIdMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var headerValue = context.Request.Headers[HeaderName].FirstOrDefault();
        var correlationId = IsValidCorrelationId(headerValue)
            ? headerValue!
            : ResolveFallback(context);

        context.Items[HttpContextItemKey] = correlationId;
        context.Response.Headers[HeaderName] = correlationId;

        using (Serilog.Context.LogContext.PushProperty(HttpContextItemKey, correlationId))
        {
            await _next(context);
        }
    }

    private static string ResolveFallback(HttpContext context)
    {
        // ASP.NET Core normally populates TraceIdentifier with a hex GUID.
        // Defensive fallback if a custom test host leaves it empty.
        return string.IsNullOrWhiteSpace(context.TraceIdentifier)
            ? Guid.NewGuid().ToString()
            : context.TraceIdentifier;
    }

    private static bool IsValidCorrelationId(string? correlationId)
    {
        if (string.IsNullOrWhiteSpace(correlationId) || correlationId.Length > MaxLength)
        {
            return false;
        }

        foreach (var c in correlationId)
        {
            // Charset lookup is faster than per-char string allocations.
            if (c >= AllowedCharLookup.Length || !AllowedCharLookup[c])
            {
                return false;
            }
        }

        return true;
    }

    private static bool[] BuildLookup(string allowed)
    {
        var lookup = new bool[128];
        foreach (var c in allowed)
        {
            if (c < lookup.Length)
            {
                lookup[c] = true;
            }
        }
        return lookup;
    }
}
