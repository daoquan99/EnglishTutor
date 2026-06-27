using Microsoft.AspNetCore.Http;
using EnglishTutor.BuildingBlocks.Application.DomainEvents;
using System;

namespace EnglishTutor.BuildingBlocks.Infrastructure.Correlation;

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

    public async Task InvokeAsync(HttpContext context, IEventEnvelopeContextSetter setter)
    {
        var headerValue = context.Request.Headers[HeaderName].FirstOrDefault();
        var correlationIdStr = IsValidCorrelationId(headerValue)
            ? headerValue!
            : ResolveFallback(context);

        context.Items[HttpContextItemKey] = correlationIdStr;
        context.Response.Headers[HeaderName] = correlationIdStr;

        if (!Guid.TryParse(correlationIdStr, out var correlationId))
        {
            correlationId = Guid.NewGuid();
        }

        setter.Set(new EventEnvelopeContext(
            CorrelationId: correlationId,
            CausationId: null));

        using (Serilog.Context.LogContext.PushProperty(HttpContextItemKey, correlationIdStr))
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
