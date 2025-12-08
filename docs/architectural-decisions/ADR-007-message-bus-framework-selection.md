# ADR-007: Message Bus and Saga Orchestration Framework Selection

**Author:** Mohammad Shakhtour  
**Status:** Accepted  

## Summary

MassTransit chosen for this repository

## Context

In a microservices architecture, services need to communicate asynchronously via a message bus. We require:

1. **Message Bus Communication** - Reliable, guaranteed delivery of events between services
2. **Saga Orchestration** - Coordinating distributed workflows across multiple services
3. **Retry Mechanisms** - Handling transient failures with configurable retry policies
4. **Error Handling** - Dead letter queues, fault handling, and compensation
5. **Monitoring and Traceability** - Observability into message flow, saga state, and failures
6. **Production Readiness** - Enterprise features for mission-critical systems

The original template used an in-process outbox pattern with background workers. For microservices, we need a framework that can:
- Publish messages to a message broker (RabbitMQ, Azure Service Bus, etc.)
- Handle saga orchestration with state persistence
- Provide retry policies and error handling
- Support monitoring and debugging in production

## Decision Drivers

- **Cost** - Open-source vs commercial licensing
- **Enterprise Features** - Monitoring, tooling, support, and production-grade capabilities
- **Development Complexity** - Learning curve and implementation effort
- **Production Readiness** - Reliability, scalability, and operational tooling
- **Community and Support** - Documentation, community help, and professional support
- **Transport Flexibility** - Support for multiple message brokers
- **Saga Support** - Built-in saga orchestration capabilities
- **Migration Path** - Ability to switch frameworks if needed

## Considered Options

### Option 1: MassTransit

**Description:**  
Open-source .NET messaging framework that provides abstractions over message brokers and supports saga orchestration.

**Features:**
- Supports multiple transports
- Saga support via state machines 
- Built-in retry mechanisms 
- Message scheduling
- Consumer pipeline with middleware support
- Open-source and free

**Pros:**
- **No licensing costs** - Free and open-source
- **Transport flexibility** - Easy to switch between RabbitMQ, Azure Service Bus, etc.
- **Active community** - Good GitHub activity and community support
- **Saga support** - State machines
- **Retry policies** - Configurable retry with exception filtering
- **Learning curve** - Reasonable for developers familiar with .NET

**Cons:**
- **Limited monitoring tools** - No built-in enterprise monitoring solution
- **Debugging complexity** - Harder to trace message flow and saga state
- **Production tooling** - Limited compared to commercial solutions

### Option 2: NServiceBus

**Description:**  
Commercial .NET messaging framework with enterprise-grade tooling and support from Particular Software.

**Features:**
- Supports multiple transports (RabbitMQ, Azure Service Bus, SQL Server, etc.)
- Advanced saga orchestration with automatic state persistence
- **Monitoring** - Visual message flow , real-time monitoring and alerting dashboard
- **ServiceControl** - Centralized message audit and error management
- Production-proven at scale

**Pros:**
- **Enterprise monitoring** - ServiceInsight provides visual message flow tracing
- **Production tooling** - ServicePulse for real-time monitoring and alerting
- **Excellent debugging** - ServiceInsight shows complete message journey and saga state
- **Professional support** - Commercial support from Particular Software
- **Advanced retry policies** - good error handling, dead letter queue management and manual retry support
- **Saga orchestration** - Excellent saga support with automatic state management
- **Observability** - Built-in metrics, tracing, and monitoring

**Cons:**
- **Cost** - Commercial license required (not free)
- **Learning curve** - More complex than MassTransit, requires training
- **Overhead** - Additional infrastructure (ServiceControl, ServicePulse) to manage


### Option 3: Custom Implementation

**Description:**  
Build messaging infrastructure from scratch using raw message broker APIs and custom saga orchestration.

**Features:**
- Full control over implementation
- No framework dependencies
- Custom retry logic
- Custom saga state management
- Custom monitoring and logging

**Pros:**
- **Full control** - Complete control over every aspect
- **No licensing** - No framework licensing costs
- **Lightweight** - Only what you need, no framework overhead
- **Customization** - Tailored to exact requirements

**Cons:**
- **Development effort** - Significant time to build and test
- **Maintenance burden** - Ongoing maintenance and bug fixes
- **Edge cases** - Need to handle all failure scenarios
- **Production risks** - Higher risk of bugs and edge cases
- **No tooling** - Need to build monitoring and debugging tools
- **Reinventing the wheel** - Solving problems already solved by frameworks

**Why Not:**
Building messaging infrastructure from scratch is a significant undertaking. Frameworks like MassTransit and NServiceBus have solved complex problems.

The development and maintenance effort far outweighs the benefits of full control.

## Decision Outcome

**Chosen for this repository: MassTransit**

**Reasoning:**
1. **Open-source and free** - Suitable for demos, learning, and open-source projects
2. **Good enough features** - Provides saga orchestration, retry, and basic monitoring
3. **Transport flexibility** - Easy to switch transports if needed

**But I would recommend NServiceBus for production and enterprise applications**


## Migration Path

### From MassTransit to NServiceBus

The migration is feasible because:
1. **Similar concepts** - Both use similar patterns (consumers, sagas, retry policies)
2. **Transport abstraction** - Both abstract the underlying message broker
3. **Saga patterns** - Both support saga orchestration
4. **Event contracts** - Message contracts can remain largely unchanged
5. **Template Abstraction** - our template already abstract the outbox which make it easier to change the bridge.

**Migration Steps:**
1. Replace MassTransit dependencies with NServiceBus
2. Replcae CleanArchitecture.Outbox.MassTransit.Bridge with NServicebus bridge implementation