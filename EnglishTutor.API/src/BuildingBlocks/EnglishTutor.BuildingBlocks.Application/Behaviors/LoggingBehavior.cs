using System.Diagnostics;
using EnglishTutor.BuildingBlocks.Application.Results;
using MediatR;
using Microsoft.Extensions.Logging;

namespace EnglishTutor.BuildingBlocks.Application.Behaviors;

public sealed class LoggingBehavior<TRequest, TResponse>(
    ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : class
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;

        logger.LogInformation("Handling {RequestName}", requestName);

        var stopwatch = Stopwatch.StartNew();
        var response = await next(cancellationToken);
        stopwatch.Stop();

        if (stopwatch.ElapsedMilliseconds > 500)
        {
            logger.LogWarning("Long running request: {RequestName} ({ElapsedMs}ms)", requestName, stopwatch.ElapsedMilliseconds);
        }

        if (response is Result { IsFailure: true } failureResult)
        {
            logger.LogWarning("Request {RequestName} failed: {ErrorCode} - {ErrorMessage}",
                requestName, failureResult.Error.Code, failureResult.Error.Message);
        }
        else
        {
            logger.LogInformation("Handled {RequestName} in {ElapsedMs}ms", requestName, stopwatch.ElapsedMilliseconds);
        }

        return response;
    }
}
