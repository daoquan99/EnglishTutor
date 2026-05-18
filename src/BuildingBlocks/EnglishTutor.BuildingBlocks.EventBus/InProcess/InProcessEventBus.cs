using System.Collections.Concurrent;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace EnglishTutor.BuildingBlocks.EventBus.InProcess;

public sealed class InProcessEventBus(
    IServiceProvider serviceProvider,
    ILogger<InProcessEventBus> logger) : IEventBus
{
    private static readonly ConcurrentDictionary<Type, MethodInfo> HandleMethodCache = new();

    public async Task PublishAsync(IIntegrationEvent @event, CancellationToken ct = default)
    {
        var eventType = @event.GetType();
        var handlerType = typeof(IIntegrationEventHandler<>).MakeGenericType(eventType);
        var handlers = serviceProvider.GetServices(handlerType);

        foreach (var handler in handlers)
        {
            var handleMethod = HandleMethodCache.GetOrAdd(
                handlerType,
                type => type.GetMethod(nameof(IIntegrationEventHandler<IntegrationEvent>.HandleAsync))!);

            logger.LogInformation("Dispatching {EventType} to {HandlerType}", eventType.Name, handler!.GetType().Name);

            try
            {
                await (Task)handleMethod.Invoke(handler, [@event, ct])!;
            }
            catch (TargetInvocationException ex) when (ex.InnerException is not null)
            {
                logger.LogError(ex.InnerException, "Error handling {EventType} in {HandlerType}", eventType.Name, handler.GetType().Name);
                throw ex.InnerException;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error handling {EventType} in {HandlerType}", eventType.Name, handler.GetType().Name);
                throw;
            }
        }
    }
}
