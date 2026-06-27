using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Policy;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace EnglishTutor.BuildingBlocks.Presentation.Responses;

public static class ApiPresentationServiceCollectionExtensions
{
    public static IServiceCollection AddApiPresentation(this IServiceCollection services)
    {
        services.AddExceptionHandler(options =>
        {
            options.ExceptionHandler = async context =>
            {
                var exception = context.Features.Get<IExceptionHandlerFeature>()?.Error;
                if (exception is BadHttpRequestException badRequest)
                {
                    await ApiResults.Problem(
                        statusCode: StatusCodes.Status400BadRequest,
                        code: ApiErrorCodes.InvalidRequest,
                        title: "Invalid request",
                        message: badRequest.Message)
                        .ExecuteAsync(context);
                    return;
                }

                var logger = context.RequestServices.GetRequiredService<ILogger<ApiExceptionHandler>>();
                if (exception is not null)
                {
                    logger.LogError(exception, "Unhandled exception while processing request.");
                }

                await ApiResults.Problem(
                    statusCode: StatusCodes.Status500InternalServerError,
                    code: ApiErrorCodes.Unexpected,
                    title: "Unexpected error",
                    message: "An unexpected server error occurred.")
                    .ExecuteAsync(context);
            };
        });
        services.AddSingleton<IAuthorizationMiddlewareResultHandler, ApiAuthorizationMiddlewareResultHandler>();
        services.ConfigureHttpJsonOptions(options =>
            options.SerializerOptions.Converters.Add(
                new JsonStringEnumConverter(JsonNamingPolicy.CamelCase)));
        return services;
    }
}
