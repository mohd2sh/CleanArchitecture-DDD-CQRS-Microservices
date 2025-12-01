using System.Text.Json;
using System.Text.Json.Serialization;
using CleanArchitecture.Core.Application.Abstractions.Events;
using CleanArchitecture.Core.Application.Abstractions.Messaging;
using CleanArchitecture.Core.Application.Abstractions.Persistence;
using CleanArchitecture.Core.Domain.Abstractions;
using CleanArchitecture.Outbox.Abstractions;
using Microsoft.Extensions.Logging;

namespace CleanArchitecture.Core.Application.Pipelines;

/// <summary>
/// Pipeline behavior for integration event handlers that collects domain events raised by handlers,
/// processes them transactionally (publishes to in-process handlers), and writes them to the outbox.
/// Similar to DomainEventsPipeline for commands, but for integration event handlers.
/// </summary>
public class DomainEventsIntegrationEventPipeline<TEvent> : IIntegrationEventPipeline<TEvent>
{
    private readonly IUnitOfWork _uow;
    private readonly IDomainEventDispatcher _eventDispatcher;
    private readonly IOutboxStore _outboxStore;
    private readonly ILogger<DomainEventsIntegrationEventPipeline<TEvent>> _logger;

    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        IncludeFields = false,
        DefaultIgnoreCondition = JsonIgnoreCondition.Never
    };

    public DomainEventsIntegrationEventPipeline(
        IUnitOfWork uow,
        IDomainEventDispatcher eventDispatcher,
        IOutboxStore outboxStore,
        ILogger<DomainEventsIntegrationEventPipeline<TEvent>> logger)
    {
        _uow = uow;
        _eventDispatcher = eventDispatcher;
        _outboxStore = outboxStore;
        _logger = logger;
    }

    public async Task Handle(
        TEvent @event,
        PipelineDelegate next,
        CancellationToken cancellationToken = default)
    {
        // Execute the handler first (domain operations happen here)
        await next();

        // Accumulate integration events
        var integrationEvents = new List<IDomainEvent>();

        // Process events in batches until no new events are raised
        await ProcessDomainEventsInBatches(integrationEvents, cancellationToken);

        // Only write to outbox after ALL transactional processing succeeds
        await WriteIntegrationEventsToOutbox(integrationEvents, cancellationToken);
    }

    private async Task ProcessDomainEventsInBatches(
        List<IDomainEvent> integrationEvents,
        CancellationToken cancellationToken)
    {
        var depth = 1;

        while (true)
        {
            // Collect and clear events from all aggregates
            var domainEvents = _uow.CollectDomainEvents();

            if (domainEvents.Count == 0)
                break; // No more events to process

            _logger.LogDebug("Processing domain event depth {Depth} with {EventCount} events",
                depth, domainEvents.Count);

            foreach (var domainEvent in domainEvents)
            {
                // Publish to transactional handlers using dispatcher
                await _eventDispatcher.PublishAsync((dynamic)domainEvent, cancellationToken);

                // Always accumulate for integration handlers (deferred, via outbox)
                integrationEvents.Add(domainEvent);
            }

            depth++;
        }
    }

    private async Task WriteIntegrationEventsToOutbox(
        List<IDomainEvent> integrationEvents,
        CancellationToken cancellationToken)
    {
        if (integrationEvents.Count == 0)
            return;

        _logger.LogDebug("Writing {Count} integration events to outbox", integrationEvents.Count);

        foreach (var domainEvent in integrationEvents)
        {
            var eventType = domainEvent.GetType();

            await _outboxStore.AddAsync(new OutboxMessage
            {
                Id = Guid.NewGuid(),
                EventType = eventType.AssemblyQualifiedName!,
                Payload = JsonSerializer.Serialize(domainEvent, eventType, SerializerOptions),
                CreatedAt = DateTime.UtcNow
            }, cancellationToken);
        }
    }
}

