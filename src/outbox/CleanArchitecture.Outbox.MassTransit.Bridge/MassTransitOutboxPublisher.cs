using CleanArchitecture.Outbox.Abstractions;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace CleanArchitecture.Outbox.MassTransit.Bridge;

/// <summary>
/// MassTransit implementation of IOutboxPublisher.
/// Publishes integration events to RabbitMQ via MassTransit.
/// </summary>
public sealed class MassTransitOutboxPublisher : IOutboxPublisher
{
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ILogger<MassTransitOutboxPublisher> _logger;

    public MassTransitOutboxPublisher(
        IPublishEndpoint publishEndpoint,
        ILogger<MassTransitOutboxPublisher> logger)
    {
        _publishEndpoint = publishEndpoint;
        _logger = logger;
    }

    public async Task PublishAsync<TEvent>(TEvent integrationEvent, CancellationToken cancellationToken = default)
        where TEvent : class
    {
        ArgumentNullException.ThrowIfNull(integrationEvent);

        _logger.LogDebug(
            "Publishing integration event {EventType} to message bus",
            typeof(TEvent).Name);

        try
        {
            await _publishEndpoint.Publish(integrationEvent, cancellationToken);

            _logger.LogDebug(
                "Successfully published integration event {EventType} to message bus",
                typeof(TEvent).Name);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to publish integration event {EventType} to message bus",
                typeof(TEvent).Name);
            throw;
        }
    }
}





