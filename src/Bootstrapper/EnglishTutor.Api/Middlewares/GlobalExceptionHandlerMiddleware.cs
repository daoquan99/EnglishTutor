using System.Net;
using System.Text.Json;
using EnglishTutor.BuildingBlocks.Application.Exceptions;
using EnglishTutor.BuildingBlocks.Domain.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace EnglishTutor.Api.Middlewares;

public sealed class GlobalExceptionHandlerMiddleware(
    RequestDelegate next,
    ILogger<GlobalExceptionHandlerMiddleware> logger)
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var correlationId = context.Items["CorrelationId"]?.ToString() ?? "N/A";

        var (statusCode, title, detail) = exception switch
        {
            BusinessRuleValidationException ex => (HttpStatusCode.UnprocessableEntity, "Business rule validation failed.", ex.Details),
            NotFoundException ex => (HttpStatusCode.NotFound, "Resource not found.", ex.Message),
            ValidationException => (HttpStatusCode.UnprocessableEntity, "Validation failed.", "One or more validation errors occurred."),
            UnauthorizedAccessException ex => (HttpStatusCode.Unauthorized, "Unauthorized.", ex.Message),
            _ => (HttpStatusCode.InternalServerError, "Internal server error.", "An unexpected error occurred.")
        };

        logger.LogError(exception, "Unhandled exception. CorrelationId: {CorrelationId}, StatusCode: {StatusCode}",
            correlationId, (int)statusCode);

        if (context.Response.HasStarted)
        {
            logger.LogWarning("Response already started, cannot write error response. CorrelationId: {CorrelationId}", correlationId);
            return;
        }

        context.Response.StatusCode = (int)statusCode;
        context.Response.ContentType = "application/problem+json";

        var problemDetails = new ProblemDetails
        {
            Status = (int)statusCode,
            Title = title,
            Detail = detail,
            Instance = context.Request.Path
        };

        problemDetails.Extensions["correlationId"] = correlationId;

        if (exception is ValidationException validationException)
        {
            problemDetails.Extensions["errors"] = validationException.Failures
                .GroupBy(f => f.PropertyName)
                .ToDictionary(
                    group => group.Key,
                    group => group.Select(f => f.ErrorMessage).Distinct().ToArray());
        }

        await context.Response.WriteAsync(JsonSerializer.Serialize(problemDetails, JsonOptions));
    }
}
