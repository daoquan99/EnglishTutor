using System.Text.Json;
using Microsoft.AspNetCore.Http;

namespace EnglishTutor.BuildingBlocks.Presentation.Responses;

internal sealed class ApiHttpResult<T> : IResult
{
    private const string CorrelationHeaderName = "X-Correlation-Id";

    private readonly int _statusCode;
    private readonly T? _data;
    private readonly ApiProblem? _error;
    private readonly ApiMetadata? _meta;
    private readonly string? _location;

    public ApiHttpResult(
        int statusCode,
        T? data,
        ApiProblem? error,
        ApiMetadata? meta,
        string? location = null)
    {
        _statusCode = statusCode;
        _data = data;
        _error = error;
        _meta = meta;
        _location = location;
    }

    public async Task ExecuteAsync(HttpContext httpContext)
    {
        ArgumentNullException.ThrowIfNull(httpContext);

        httpContext.Response.StatusCode = _statusCode;
        httpContext.Response.ContentType = "application/json; charset=utf-8";

        if (_location is not null)
        {
            httpContext.Response.Headers.Location = _location;
        }

        var error = _error is null
            ? null
            : _error with
            {
                TraceId = httpContext.TraceIdentifier,
                CorrelationId = ResolveCorrelationId(httpContext)
            };

        var response = new ApiResponse<T>(
            Success: error is null,
            Data: error is null ? _data : default,
            Error: error,
            Meta: error is null ? _meta : null);

        await JsonSerializer.SerializeAsync(
            httpContext.Response.Body,
            response,
            ApiJsonOptions.Default,
            httpContext.RequestAborted);
    }

    private static string ResolveCorrelationId(HttpContext httpContext)
    {
        var value = httpContext.Response.Headers[CorrelationHeaderName].ToString();
        return string.IsNullOrWhiteSpace(value) ? httpContext.TraceIdentifier : value;
    }
}
