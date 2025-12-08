# Clean Architecture CMMS: Microservices Migration

A .NET 8 microservices architecture demonstrating the evolution from a monolithic DDD/CQRS template to a fully distributed system. This repository showcases how the original template's design decisions enabled a smooth migration to microservices.

**Original Template:** [CleanArchitecture-DDD-CQRS](https://github.com/mohd2sh/CleanArchitecture-DDD-CQRS-Microservices)

## Introduction

This repository contains a **Computerized Maintenance Management System (CMMS)** implemented as microservices. It demonstrates a complete migration from the original monolithic template, showing how the foundational architecture patterns made this transition straightforward.

**Note:** This solution contains all services in a single repository for architecture showcase purposes.

### CMMS Domain Context

A CMMS system manages maintenance operations:
- **Assets** - Equipment, machinery, or facilities requiring maintenance
- **Work Orders** - Maintenance tasks, repairs, or inspections
- **Technicians** - Skilled workers who perform maintenance
- **Assignments** - Connections between technicians and work orders

The system handles the complete maintenance lifecycle: creating work orders, assigning technicians, tracking progress, and completing work across distributed services.

## Evolution from Monolith to Microservices

This repository demonstrates the migration journey from the original template. The original template was designed with microservices in mind, which made the migration straightforward. Here's what enabled this:

### What Made the Original Template Microservices-Ready

**1. Outbox Pattern Abstraction**
The original template implemented `IOutboxStore` and `IOutboxPublisher` abstractions. This made swapping from in-process handlers to message bus integration trivial. The same code that worked in-process now publishes to RabbitMQ via MassTransit.

**2. Event-Driven Architecture**
The dual handler system (`IDomainEventHandler` for transactional events, `IIntegrationEventHandler` for async events) translated directly to microservices. Integration events that were handled in-process now flow through the message bus to other services.

**3. Bounded Context Isolation**
Architecture tests enforced strict boundaries between bounded contexts. When we split into services, these boundaries were already respected, preventing cross-service coupling.

**4. Database-Per-Service Structure**
The original template used separate database schema for each domain, and with no direct referince between domin/schema

**5. Clean Separation of Concerns**
Domain, Application, and Infrastructure layers were clearly separated. Each service maintains this structure independently.

**6. Architecture Tests**
Automated tests enforced boundaries at compile-time. These same tests now ensure services don't accidentally depend on each other's internals.

## Architecture Diagrams

### High-Level System Architecture

![Microservices Architecture](docs/diagrams/HighLevelArch.png)

The system architecture demonstrates a microservices implementation with four main services:

**WorkOrders Service** - Manages work order lifecycle, handles creation, assignment, and completion. Publishes events when work orders are created, assigned, or completed.

**Assets Service** - Manages asset information and maintenance status. Responds to work order events to update asset status and tracks maintenance history.

**Technicians Service** - Manages technician information and availability. Handles assignment tracking and validates technician assignments.

**Orchestration Service** - Manages distributed workflows using saga pattern. Coordinates multi-step operations across services and handles compensation for failed operations.

Each service follows Clean Architecture with Domain, Application, Infrastructure, and API layers. Services maintain their own databases (`WorkOrdersDb`, `AssetsDb`, `TechniciansDb`, `OrchestrationDb`) ensuring service autonomy and independent scaling.

The architecture includes:
- **API Gateway Layer**: Kong for routing and unified API documentation
- **Microservices Layer**: Four independent services each following Clean Architecture
- **Messaging Infrastructure**: RabbitMQ message bus with MassTransit bridge and shared event contracts
- **Data Layer**: Database-per-service pattern with separate write and read databases

Services communicate asynchronously via events through the message bus. The Orchestration Service manages distributed workflows using the Saga pattern.

### Assign Technician Flow

![Assign Technician Flow](docs/diagrams/AssignTechnicianFlow.png)

This diagram illustrates the complete flow for assigning a technician to a work order:
1. API request routed through API Gateway to WorkOrders Service
2. Work order updated and event written to outbox (same transaction)
3. Outbox processor publishes event to RabbitMQ
4. Orchestration Service (Saga) receives event and orchestrates the workflow
5. Saga publishes event to Technicians Service for validation
6. Technicians Service validates and responds
7. Saga finalizes upon successful validation

The flow demonstrates the outbox pattern for guaranteed delivery, saga orchestration for distributed transactions, and eventual consistency across services.

## Inter-Service Communication

### MassTransit Bridge

We use MassTransit as a bridge on top of our outbox pattern. The `CleanArchitecture.Outbox.MassTransit.Bridge` package implements `IOutboxPublisher` to publish events to RabbitMQ.

**Why MassTransit?**
- Open-source solution suitable for demos and development
- Good integration with .NET
- Supports RabbitMQ, Azure Service Bus, and other transports

The outbox pattern abstraction from the original template made this integration straightforward. We swapped the in-process publisher for a MassTransit publisher without changing any business logic.

### Shared Event Contracts

The `CleanArchitecture.Cmms.Contracts` project contains immutable event schemas shared across services:
- `WorkOrders.Events` - Events published by WorkOrders service
- `Assets.Events` - Events published by Assets service
- `Technicians.Events` - Events published by Technicians service

All services reference this project to ensure consistent message contracts. Events are versioned and immutable once published.

### Event Flow

1. Service publishes integration event → written to outbox table (same transaction)
2. Outbox processor picks up event → publishes to RabbitMQ via MassTransit
3. Other services consume from RabbitMQ → invoke integration event handlers
4. Handlers update local service state

This ensures guaranteed delivery with at-least-once semantics.

## Orchestration Service

The Orchestration service manages distributed workflows using the saga pattern. It coordinates operations that span multiple services and handles eventual consistency.

### Saga Pattern

Sagas manage long-running transactions across services like:
- **AssignTechnicianSaga** - Coordinates technician assignment validation
- **CompleteWorkOrderSaga** - Coordinates work order completion across Assets and Technicians services

### Eventual Consistency

Operations that require coordination across services are eventually consistent:
- Work order completion triggers parallel updates in Assets and Technicians services
- Saga waits for both services to complete
- If one fails, compensation events are published

### Compensation Events

When operations fail, the saga publishes compensation like for example:
- `WorkOrderCompletingFailedEvent` - Triggers rollback in participating services
- `RevertTechnicianAssignmentRequestedEvent` - Reverts technician assignment

This ensures system consistency even when operations fail partway through.

### MassTransit State Machines

Sagas are implemented using MassTransit state machines, which provide:
- State persistence
- Timeout handling
- Retry
- Fault handling

## Cross-Service Query Challenge

One challenge in microservices is querying data across service boundaries. Each service has its own database, so traditional joins aren't possible.

### The Problem

In `WorkOrderReadRepository.cs`, we need to display work orders with technician and asset names. In a monolith, this would be a simple join. In microservices, the data lives in different databases.

**Current Approach (TODO):**
Cross-database joins as an interim solution. This works when services share the same database server but violates service autonomy.

**Future Approach:**
CDC (Change Data Capture) with Debezium streaming from SQL Server to Elasticsearch. This provides:
- Service autonomy (no cross-database dependencies)
- Optimized read models
- Eventual consistency
- Better scalability

We have a separate demo repository showing the CDC approach with Debezium.

## Local Infrastructure Components

### API Gateway (Kong)

Kong routes requests to the appropriate service:
- `/api/v1/workorders` → WorkOrders Service
- `/api/v1/assets` → Assets Service
- `/api/v1/technicians` → Technicians Service

### Message Bus (RabbitMQ)

RabbitMQ handles inter-service communication:
- Events published to exchanges
- Services consume from queues
- Guaranteed delivery with retry

### Database

SQL Server instances (one per service):
- Each service has its own database
- Independent migrations
- Service autonomy

### Docker Compose

All infrastructure is containerized:
- Services run as containers
- RabbitMQ, SQL Server, Kong included
- Easy local development setup

## Project Structure

```
src/
├── core/                                    # Core Framework
│   ├── CleanArchitecture.Core.Application
│   ├── CleanArchitecture.Core.Domain
│   ├── CleanArchitecture.Core.Infrastructure
│   ├── CleanArchitecture.Core.Application.Pipelines
│   └── CleanArchitecture.Core.Api
│
├── Contracts/                               # Shared Event Contracts
│   └── CleanArchitecture.Cmms.Contracts
│       ├── WorkOrders/Events/
│       ├── Assets/Events/
│       └── Technicians/Events/
│
├── outbox/                                   # Outbox Pattern
│   ├── CleanArchitecture.Outbox.Abstractions
│   ├── CleanArchitecture.Outbox
│   └── CleanArchitecture.Outbox.MassTransit.Bridge
│
└── services/                                 # Microservices
    ├── WorkOrders.Service/
    │   ├── CleanArchitecture.Cmms.Domain.WorkOrders
    │   ├── CleanArchitecture.Cmms.Application.WorkOrders
    │   ├── CleanArchitecture.Cmms.Infrastructure.WorkOrders
    │   └── CleanArchitecture.Cmms.Api.WorkOrders
    │
    ├── Assets.Service/
    │   ├── CleanArchitecture.Cmms.Domain.Assets
    │   ├── CleanArchitecture.Cmms.Application.Assets
    │   ├── CleanArchitecture.Cmms.Infrastructure.Assets
    │   └── CleanArchitecture.Cmms.Api.Assets
    │
    ├── Technicians.Service/
    │   ├── CleanArchitecture.Cmms.Domain.Technicians
    │   ├── CleanArchitecture.Cmms.Application.Technicians
    │   ├── CleanArchitecture.Cmms.Infrastructure.Technicians
    │   └── CleanArchitecture.Cmms.Api.Technicians
    │
    └── Orchestration.Service/
        └── Orchestration/                   # Saga orchestration

tests/
├── [Service].Domain.UnitTests/
├── [Service].Application.UnitTests/
└── [Service].Api.IntegrationTests/
```

## Key Design Decisions

### 1. Database-Per-Service

Each service owns its data. This ensures:
- Service autonomy
- Independent scaling
- Technology flexibility per service
- Clear ownership boundaries

### 2. Event-Driven Communication

Services communicate via events, not direct calls:
- Loose coupling
- Better scalability
- Resilience to service failures
- Natural evolution path

### 3. Outbox Pattern

Integration events written to outbox in same transaction:
- Guaranteed delivery
- At-least-once semantics
- Survives application restarts
- Works with any message bus

### 4. Saga Orchestration

Complex workflows coordinated by orchestration service:
- Manages eventual consistency
- Handles compensation
- Provides visibility into distributed operations

### 5. Shared Contracts

Immutable event contracts shared across services:
- Type safety
- Versioning support
- Clear service boundaries
- Compile-time validation

## Architectural Decision Records

The original template's ADRs (ADR-001 through ADR-006) documented foundational patterns that enabled this migration:
- Outbox pattern
- Domain vs Integration events
- Cross-aggregate coordination
- Error management

**New ADRs for microservices:**
- [ADR-007: Message Bus and Saga Orchestration Framework Selection](docs/architectural-decisions/ADR-007-message-bus-framework-selection.md) - MassTransit vs NServiceBus comparison
- [ADR-008: Cross-Service Query Patterns](docs/architectural-decisions/ADR-008-cross-service-query-patterns.md) - Options for querying data across service boundaries (Status: Open/In-Progress)

## Quick Start

### Prerequisites

- .NET 8 SDK
- Docker Desktop
- Visual Studio 2022 or VS Code

### Docker Compose

The easiest way to run all services:

```bash
git clone <repository-url>
cd Company.Cmms
docker-compose up
```

**What's included:**
- SQL Server container (shared instance, separate databases per service)
- RabbitMQ container
- Kong API Gateway
- All microservices
- Swagger UI (unified API documentation)

**Access:**
- API Gateway: http://localhost:8000
- Swagger UI: http://localhost:8081
- RabbitMQ Management: http://localhost:15672 (guest/guest)

### Local Development

Run services individually:

```bash
# WorkOrders Service
cd src/services/WorkOrders.Service/CleanArchitecture.Cmms.Api.WorkOrders
dotnet run

# Assets Service
cd src/services/Assets.Service/CleanArchitecture.Cmms.Api.Assets
dotnet run

# Technicians Service
cd src/services/Technicians.Service/CleanArchitecture.Cmms.Api.Technicians
dotnet run
```

Each service requires:
- SQL Server connection string
- RabbitMQ connection string

## Testing Strategy

### Unit Tests

Each service has unit tests for:
- Domain logic
- Application handlers
- Architecture boundaries

### Integration Tests

Integration tests use Testcontainers for:
- Real database testing
- End-to-end scenarios
- Event flow validation

### Architecture Tests

Automated tests enforce:
- Service boundaries
- Layer dependencies
- CQRS separation
- Bounded context isolation

## What's Not Included

This repository focuses on architecture and design. concerns that are out of scope:
- Authentication/Authorization
- Distributed tracing
- Monitoring and alerting
- Production infrastructure

## TODO

Future improvements and enhancements:

- Complete Kong API gateway configuration and routing
- Cross-Service Query Patterns
- Add Orchestration unit tests and integration tests
- Complete docker compose setup for all services

## Contributing

Contributions welcome. This repository serves as an architecture reference and learning resource.

## License

MIT License - see [LICENSE](LICENSE) file for details.

---

**Built to demonstrate microservices architecture patterns in .NET**
