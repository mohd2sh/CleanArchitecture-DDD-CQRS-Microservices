# ADR-008: Cross-Service Query Patterns

**Author:** Mohammad Shakhtour  
**Status:** Open/In-Progress

## Summary

Decision pending on how to handle cross-service queries in microservices architecture. Current implementation returns empty strings for related data from other services. Multiple options are being evaluated.

## Context

In a microservices architecture, each service owns its own database. This creates a challenge when we need to display data that spans multiple services.

**The Problem:**

In `WorkOrderReadRepository.GetActiveWithTechnicianAndAssetAsync`, we need to display work orders with technician names and asset names. In the original repo, this would be a SQL join across schema. However, in this microservices repo:

- Work orders are stored in the `WorkOrders` service database
- Technician data is stored in the `Technicians` service database  
- Asset data is stored in the `Assets` service database

**Current State:**

The current implementation returns empty strings for `TechnicianName` and `AssetName`:

```csharp
SELECT 
    w.Id,
    w.Title,
    '' AS TechnicianName,  
    '' AS AssetName,
    w.Status
FROM workorders.WorkOrders w
WHERE w.Status <> @Completed
```

We need to decide on an approach to populate this cross-service data while maintaining service autonomy and architectural boundaries.

## Decision Drivers

- **Service Autonomy** - Each service should own its data and not depend on other services' databases
- **Complexity** - Implementation and maintenance complexity
- **Latency** - Query performance and response times
- **Scalability** - Ability to scale independently
- **Implementation Effort** - Time and resources required to implement
- **Operational Overhead** - Infrastructure and maintenance requirements

## Considered Options

### Option 1: Cross-Database Joins (Avoid! Bad Practice)

**Description:**  
Direct SQL joins across databases on the same SQL Server instance using three-part naming (`Database.Schema.Table`).

**Implementation:**
```sql
SELECT 
    w.Id,
    w.Title,
    t.Name AS TechnicianName,
    a.Name AS AssetName,
    w.Status
FROM workorders.WorkOrders w
LEFT JOIN technicians.Technicians t ON w.TechnicianId = t.Id
LEFT JOIN assets.Assets a ON w.AssetId = a.Id
WHERE w.Status <> @Completed
```

**Pros:**
- Simple to implement - just SQL joins
- Better consistency - read replica sync faster than CDC streaming
- No additional infrastructure required
- Low latency - single query

**Cons:**
- **Bad practice** - Violates service autonomy and database-per-service principle
- Deployment coupling - All databases must be on same server
- Schema changes break queries - Changes in one service affect others
- Violates microservices principles - Creates distributed monolith risk
- Not scalable - Cannot scale databases independently

**Why Not:**
Cross-database joins fundamentally violate the microservices principle of service autonomy. While they might work as a temporary solution, they create tight coupling that defeats the purpose of microservices architecture.

**Note:** This would only be considered as a temporary workaround, not a long-term solution.

### Option 2: CDC (Change Data Capture)

**Description:**  
Use Change Data Capture (CDC) to stream database changes to a read-optimized store (e.g., Elasticsearch). Services query the denormalized read model instead of their own databases.

**Implementation:**
- Debezium captures changes from SQL Server databases
- Streams changes to Kafka
- Elasticsearch consumers build denormalized read models
- Services query Elasticsearch for cross-service data

**Pros:**
- **Service autonomy maintained** - No direct database dependencies
- **Optimized read models** - Denormalized data optimized for queries
- **Scalability** - Can scale read models independently
- **Eventual consistency** - Acceptable for read scenarios
- **Separation of concerns** - Write databases for transactions, read models for queries

**Cons:**
- **Infrastructure complexity** - Requires Debezium, Kafka, Elasticsearch
- **Operational overhead** - Additional services to maintain and monitor
- **Eventual consistency** - Data may be slightly stale

**Demo Repository:**  
A working demo of CDC with Debezium streaming from SQL Server to Elasticsearch is available at: https://github.com/mohd2sh/cqrs-beyond-database

### Option 3: Data API

**Description:**  
Create a dedicated API service that aggregates and manage data from multiple services. The API makes calls to each service and composes the results.

**Pros:**
- **Service boundaries maintained** - Each service owns its data
- **Single endpoint** - Clients get all data from one API call
- **Flexibility** - Can add caching, transformation, or business logic
- **Service autonomy** - No database coupling

**Cons:**
- **Network overhead** - Multiple HTTP calls
- **Latency** - Sequential calls increase response time
- **Additional service** - Another service to develop and maintain
- **Error handling complexity** - Partial failures need handling
- **Caching complexity** - Need to cache and invalidate properly

### Option 4: Read Models / Denormalized Data

**Description:**  
Each service maintains read models of related data from other services, updated via domain events.

**Pros:**
- **Service autonomy** - No direct database access
- **Fast queries** - Local joins, no network calls
- **Event-driven** - Aligns with event-driven architecture
- **Scalability** - Each service scales independently

**Cons:**
- **Storage overhead** - Duplicated data across services
- **Custom Code** - Need to manage the event handling
- **Event handling** - Need to handle all event types
- **Data synchronization** - Complex to keep in sync
- **Storage growth** - Read models grow over time


## Decision Outcome

**Status: Open/In-Progress**

Still on the TODO. The current implementation returns empty strings for cross-service data fields.


**Considerations:**
- Cross-database joins are **bad practice** and would only be a temporary workaround if implemented
- CDC with Debezium is a strong candidate given the existing demo repository
- Data API provides a clean separation but adds latency
- Read models align well with event-driven architecture already in place
