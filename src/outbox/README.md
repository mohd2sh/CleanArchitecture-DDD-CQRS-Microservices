# Outbox Pattern

## What It Does

The outbox pattern stores integration events in a database table when business operations happen. A background worker processes these events later to ensure they're delivered even if the app crashes.

## Current Implementation

Simple setup:
- Events written to outbox table in same transaction as business data
- Background workers process events one at a time
- Failed events retry automatically
- After max retries, events move to dead letter queue
- Works with multiple workers processing in parallel without conflicts

See ADR-004 and main README for details.

## Alternative Approaches

**CDC/Streaming:**
- Read database transaction logs directly
- Stream changes to message broker
- No polling needed
- Better for microservices and high throughput

**Message Broker Bridge:**
- Keep current outbox table
- Processor publishes to message broker (RabbitMQ, Kafka, etc.)
- External services consume from broker
- Good migration path

Both approaches keep the transactional guarantee but change how events are delivered.

## Code Structure

- `CleanArchitecture.Outbox.Abstractions/` - Interfaces and entities
- `CleanArchitecture.Outbox/` - EF Core implementation and processor
- `tests/` - Integration tests

## Microservices Architecture

**Each Service MUST Have Its Own Outbox Database**

In a microservices architecture, each service must have its own `OutboxMessages` table in its own database (WriteDb). This is critical because:

1. **Transactional Guarantee**: The outbox write must be in the same database transaction as the business data write. This ensures atomicity - either both succeed or both fail.
2. **Service Autonomy**: Each service manages its own outbox independently, following the database-per-service principle.
3. **Fault Isolation**: If one service's outbox fails, others continue operating.

**Architecture:**
```
Service A (WorkOrders):
  └─ WorkOrdersDb
      ├─ WorkOrders table
      └─ OutboxMessages table  ← Same DB, same transaction

Service B (Assets):
  └─ AssetsDb
      ├─ Assets table
      └─ OutboxMessages table  ← Same DB, same transaction
```

**Message Flow:**
1. Business transaction writes to both business table and OutboxMessages (same transaction)
2. Background worker reads from OutboxMessages (after commit)
3. Worker publishes to MassTransit/RabbitMQ
4. Other services consume from message bus

## Migrations

The `OutboxDbContext` has its own migrations in the `CleanArchitecture.Outbox` assembly. Each service must apply these migrations to create the `OutboxMessages` and `DeadLetterMessages` tables.

**Example (in Program.cs):**
```csharp
if (app.Environment.IsDevelopment())
{
    using (var scope = app.Services.CreateScope())
    {
        var outboxDb = scope.ServiceProvider.GetRequiredService<OutboxDbContext>();
        await outboxDb.Database.MigrateAsync();
    }
}
```

See ADR-004 for detailed architecture decisions.






