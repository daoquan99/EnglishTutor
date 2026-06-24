using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using EnglishTutor.BuildingBlocks.Contracts.Events;
using EnglishTutor.BuildingBlocks.Domain.Aggregates;
using EnglishTutor.BuildingBlocks.Domain.DomainEvents;
using EnglishTutor.Learning.Application.Abstractions.Persistence;
using EnglishTutor.Learning.Contracts.Events;
using EnglishTutor.Learning.Domain.Aggregates.Topics.Events;
using EnglishTutor.Learning.Domain.Aggregates.ModeDefinitions.Events;
using EnglishTutor.Learning.Domain.Aggregates.Scenarios.Events;
using MassTransit;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace EnglishTutor.Learning.Infrastructure.Persistence;

/// <summary>
/// Unit of work implementation for the Learning module. Handles domain event to integration event
/// mapping, transactional outbox routing through MassTransit, and clearing aggregate domain events.
/// </summary>
internal sealed class LearningUnitOfWork : ILearningUnitOfWork
{
    private static readonly MethodInfo ClearDomainEventsMethod =
        typeof(AggregateRoot).GetMethod(nameof(AggregateRoot.ClearDomainEvents))
        ?? throw new InvalidOperationException("AggregateRoot.ClearDomainEvents not found.");

    private readonly LearningDbContext _db;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public LearningUnitOfWork(
        LearningDbContext db,
        IPublishEndpoint publishEndpoint,
        IHttpContextAccessor httpContextAccessor)
    {
        _db = db;
        _publishEndpoint = publishEndpoint;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // 1. Resolve correlation and causation contexts from HTTP headers or items
        var httpContext = _httpContextAccessor.HttpContext;
        var correlationId = ResolveCorrelationId(httpContext);
        var causationId = ResolveCausationId(httpContext);

        // 2. Extract domain events from tracked aggregate roots
        var aggregates = _db.ChangeTracker.Entries<AggregateRoot>()
            .Select(e => e.Entity)
            .Where(a => a.DomainEvents.Count > 0)
            .ToList();

        var domainEvents = aggregates
            .SelectMany(a => a.DomainEvents)
            .ToList();

        if (domainEvents.Count == 0)
        {
            return await _db.SaveChangesAsync(cancellationToken);
        }

        // 3. Start explicit transaction so MassTransit outbox filter enlists correctly
        using var transaction = await _db.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            // Map domain events to integration events and publish through MassTransit Outbox
            foreach (var domainEvent in domainEvents)
            {
                IntegrationEvent? integrationEvent = domainEvent switch
                {
                    TopicCreatedDomainEvent e => new TopicCreatedIntegrationEvent(e.TopicId, e.Name, e.Slug)
                    {
                        CorrelationId = correlationId,
                        CausationId = causationId
                    },
                    TopicUpdatedDomainEvent e => new TopicUpdatedIntegrationEvent(e.TopicId, e.Name, e.Slug, e.Description)
                    {
                        CorrelationId = correlationId,
                        CausationId = causationId
                    },
                    TopicDisabledDomainEvent e => new TopicDisabledIntegrationEvent(e.TopicId)
                    {
                        CorrelationId = correlationId,
                        CausationId = causationId
                    },
                    TopicModeEnabledDomainEvent e => new TopicModeEnabledIntegrationEvent(e.TopicId, e.ModeDefinitionId, e.ConfigJson)
                    {
                        CorrelationId = correlationId,
                        CausationId = causationId
                    },
                    TopicModeDisabledDomainEvent e => new TopicModeDisabledIntegrationEvent(e.TopicId, e.ModeDefinitionId)
                    {
                        CorrelationId = correlationId,
                        CausationId = causationId
                    },
                    ModeDefinitionCreatedDomainEvent e => new ModeDefinitionCreatedIntegrationEvent(e.ModeDefinitionId, e.Code, e.Name)
                    {
                        CorrelationId = correlationId,
                        CausationId = causationId
                    },
                    ModeDefinitionUpdatedDomainEvent e => new ModeDefinitionUpdatedIntegrationEvent(e.ModeDefinitionId, e.Code, e.Name)
                    {
                        CorrelationId = correlationId,
                        CausationId = causationId
                    },
                    ModeDefinitionDisabledDomainEvent e => new ModeDefinitionDisabledIntegrationEvent(e.ModeDefinitionId)
                    {
                        CorrelationId = correlationId,
                        CausationId = causationId
                    },
                    ScenarioCreatedDomainEvent e => new ScenarioCreatedIntegrationEvent(e.ScenarioId, e.TopicId, e.ModeDefinitionId, e.Name, e.DifficultyLevel)
                    {
                        CorrelationId = correlationId,
                        CausationId = causationId
                    },
                    ScenarioUpdatedDomainEvent e => new ScenarioUpdatedIntegrationEvent(e.ScenarioId, e.TopicId, e.ModeDefinitionId, e.Name, e.DifficultyLevel, e.PromptTemplate)
                    {
                        CorrelationId = correlationId,
                        CausationId = causationId
                    },
                    ScenarioDisabledDomainEvent e => new ScenarioDisabledIntegrationEvent(e.ScenarioId)
                    {
                        CorrelationId = correlationId,
                        CausationId = causationId
                    },
                    _ => null
                };

                if (integrationEvent is not null)
                {
                    await _publishEndpoint.Publish(integrationEvent, integrationEvent.GetType(), cancellationToken);
                }
            }

            // Save updates to database (this commits entities and maps MassTransit outbox messages in one transaction)
            var result = await _db.SaveChangesAsync(cancellationToken);

            // Commit the database transaction
            await transaction.CommitAsync(cancellationToken);

            // 5. Clear domain events on aggregate roots to avoid double-processing
            foreach (var aggregate in aggregates)
            {
                ClearDomainEventsMethod.Invoke(aggregate, null);
            }

            return result;
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    private static Guid? ResolveCorrelationId(HttpContext? httpContext)
    {
        if (httpContext is null)
        {
            return null;
        }

        if (httpContext.Request.Headers.TryGetValue("X-Correlation-Id", out var correlationHeader) &&
            Guid.TryParse(correlationHeader, out var correlationId))
        {
            return correlationId;
        }

        if (httpContext.Items.TryGetValue("CorrelationId", out var contextItem))
        {
            if (contextItem is Guid contextGuid)
            {
                return contextGuid;
            }
            if (contextItem is string contextStrVal && Guid.TryParse(contextStrVal, out var parsedGuid))
            {
                return parsedGuid;
            }
        }

        if (Guid.TryParse(httpContext.TraceIdentifier, out var traceId))
        {
            return traceId;
        }

        return null;
    }

    private static Guid? ResolveCausationId(HttpContext? httpContext)
    {
        if (httpContext is null)
        {
            return null;
        }

        if (httpContext.Request.Headers.TryGetValue("X-Causation-Id", out var causationHeader) &&
            Guid.TryParse(causationHeader, out var causationId))
        {
            return causationId;
        }

        if (Guid.TryParse(httpContext.TraceIdentifier, out var traceId))
        {
            return traceId;
        }

        return null;
    }
}
