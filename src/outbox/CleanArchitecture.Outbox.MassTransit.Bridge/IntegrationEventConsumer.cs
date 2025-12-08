using CleanArchitecture.Core.Application.Abstractions.Events;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace CleanArchitecture.Outbox.MassTransit.Bridge;

/// <summary>
/// Generic MassTransit consumer that bridges messages from RabbitMQ to IIntegrationEventDispatcher.
/// This allows existing IIntegrationEventHandler implementations to work unchanged in microservices architecture.
/// </summary>
/// <typeparam name="TEvent">The type of integration event.</typeparam>
public sealed class IntegrationEventConsumer<TEvent> : IConsumer<TEvent>
    where TEvent : class
{
    private readonly IIntegrationEventDispatcher _dispatcher;
    private readonly ILogger<IntegrationEventConsumer<TEvent>> _logger;

    public IntegrationEventConsumer(
        IIntegrationEventDispatcher dispatcher,
        ILogger<IntegrationEventConsumer<TEvent>> logger)
    {
        _dispatcher = dispatcher;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<TEvent> context)
    {
        var integrationEvent = context.Message;
        var messageId = context.MessageId ?? Guid.Empty;
        var correlationId = context.CorrelationId;

        _logger.LogInformation(
            "[CONSUMER] Received integration event {EventType} - MessageId: {MessageId}, CorrelationId: {CorrelationId}, SourceAddress: {SourceAddress}",
            typeof(TEvent).Name,
            messageId,
            correlationId,
            context.SourceAddress);

        try
        {
            _logger.LogDebug(
                "[CONSUMER] Dispatching integration event {EventType} to IIntegrationEventDispatcher",
                typeof(TEvent).Name);

            // Bridge to existing dispatcher - handlers remain unchanged!
            // Dispatcher implementation is in the CleanArchitecture.Core.Infrastructure Nuget.
            await _dispatcher.PublishAsync(integrationEvent, context.CancellationToken);

            _logger.LogInformation(
                "[CONSUMER] Successfully consumed integration event {EventType} - MessageId: {MessageId}",
                typeof(TEvent).Name,
                messageId);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "[CONSUMER] Failed to consume integration event {EventType} - MessageId: {MessageId}, CorrelationId: {CorrelationId}. Exception: {ExceptionType}, Message: {ExceptionMessage}",
                typeof(TEvent).Name,
                messageId,
                correlationId,
                ex.GetType().Name,
                ex.Message);
            throw;
        }
    }
}

