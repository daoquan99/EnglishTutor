using System.Collections.Concurrent;
using System.Linq.Expressions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace EnglishTutor.BuildingBlocks.EventBus.InProcess;

public sealed class InProcessEventBus(
    IServiceScopeFactory serviceScopeFactory,
    ILogger<InProcessEventBus> logger) : IEventBus
{
    private static readonly ConcurrentDictionary<Type, Func<object, IIntegrationEvent, CancellationToken, Task>> HandlerCache = new();

    public async Task PublishAsync(IIntegrationEvent @event, CancellationToken ct = default)
    {
        var eventType = @event.GetType();
        var handlerType = typeof(IIntegrationEventHandler<>).MakeGenericType(eventType);
        await using var scope = serviceScopeFactory.CreateAsyncScope();
        var handlers = scope.ServiceProvider.GetServices(handlerType).ToList();

        List<Exception>? failures = null;

        foreach (var handler in handlers)
        {
            var handleAsync = HandlerCache.GetOrAdd(eventType, CreateHandler);

            logger.LogInformation("Dispatching {EventType} to {HandlerType}", eventType.Name, handler!.GetType().Name);

            try
            {
                await handleAsync(handler, @event, ct);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error handling {EventType} in {HandlerType}", eventType.Name, handler.GetType().Name);
                failures ??= [];
                failures.Add(ex);
            }
        }

        if (failures is not null)
        {
            throw new AggregateException($"One or more handlers failed for {eventType.Name}.", failures);
        }
    }

    private static Func<object, IIntegrationEvent, CancellationToken, Task> CreateHandler(Type eventType)
    {
        var handlerInterface = typeof(IIntegrationEventHandler<>).MakeGenericType(eventType);
        var handleMethod = handlerInterface.GetMethod(nameof(IIntegrationEventHandler<IntegrationEvent>.HandleAsync))!;

        var handlerParameter = Expression.Parameter(typeof(object), "handler");
        var eventParameter = Expression.Parameter(typeof(IIntegrationEvent), "event");
        var cancellationTokenParameter = Expression.Parameter(typeof(CancellationToken), "ct");

        var call = Expression.Call(
            Expression.Convert(handlerParameter, handlerInterface),
            handleMethod,
            Expression.Convert(eventParameter, eventType),
            cancellationTokenParameter);

        return Expression.Lambda<Func<object, IIntegrationEvent, CancellationToken, Task>>(
                call,
                handlerParameter,
                eventParameter,
                cancellationTokenParameter)
            .Compile();
    }
}
