# CleanArchitecture.Cmms.Contracts

Shared message contracts for microservices communication.

## Purpose

This project contains event and command contracts that are shared across microservices. These contracts define the message schema for inter-service communication via MassTransit/RabbitMQ.

## Structure

```
Contracts/
├── Events/          # Integration events (broadcast)
└── Commands/        # Commands (point-to-point)
```

## Events

Events are published to exchanges (topic-based, broadcast to all subscribers).

## Commands

Commands are sent to queues (point-to-point, single consumer).

## Versioning

- Contracts should be immutable once published
- Use new event/command versions for breaking changes
- Support multiple versions during migration period

## Usage

All microservices reference this project to ensure consistent message contracts.







