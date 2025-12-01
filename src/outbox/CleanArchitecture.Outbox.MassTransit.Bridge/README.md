# CleanArchitecture.Outbox.MassTransit.Bridge

MassTransit abstractions and infrastructure for Clean Architecture microservices.

## Purpose

This package provides the bridge between the outbox pattern and MassTransit message bus, allowing events to be published to RabbitMQ while maintaining the existing `IIntegrationEventHandler` signature unchanged.

## Installation

```bash
dotnet add package Mohd2sh.CleanArchitecture.Outbox.MassTransit.Bridge
```

## Key Components

### IOutboxPublisher

Interface for publishing events from outbox to message bus.

```csharp
public interface IOutboxPublisher
{
    Task PublishAsync<TEvent>(TEvent integrationEvent, CancellationToken cancellationToken = default)
        where TEvent : class;
}
```

### MassTransitOutboxPublisher

Implementation that publishes events to MassTransit/RabbitMQ.

```csharp
services.AddScoped<IOutboxPublisher, MassTransitOutboxPublisher>();
```

### IntegrationEventConsumer

Generic consumer that bridges MassTransit messages to existing `IIntegrationEventDispatcher`.

```csharp
// Automatically discovers and consumes events
// Invokes IIntegrationEventDispatcher which triggers IIntegrationEventHandler implementations
```

## Usage

See [Microservices Migration Documentation](../../../docs/microservices-migration/04-masstransit-integration.md) for detailed usage examples.

## Repository

GitHub: https://github.com/mohd2sh/CleanArchitecture-DDD-CQRS-Microservices

## License

MIT






