# CleanArchitecture.Cmms.Contracts

Shared message contracts for microservices communication. (Can be managed by Nuget when each microservice is in separate repository))

## Purpose

This project contains event contracts that are shared across microservices. These contracts define the message schema for inter-service communication via Bus Transport.

## Events

Events are published to exchanges (topic-based, broadcast to all subscribers).


## Versioning

- Contracts should be immutable once published
- Use new event/command versions for breaking changes
- Support multiple versions during migration period

## Usage

All microservices reference this project to ensure consistent message contracts.







