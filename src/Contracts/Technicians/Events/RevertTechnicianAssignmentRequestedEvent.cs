using CleanArchitecture.Core.Domain.Abstractions;

namespace CleanArchitecture.Cmms.Contracts.Technicians.Events;

/// <summary>
/// Published by Saga orchestrator to request reversion of technician assignment (compensation).
/// Consumed by Technicians service via MassTransit message bus.
/// </summary>
public sealed record RevertTechnicianAssignmentRequestedEvent(
    Guid TechnicianId,
    Guid WorkOrderId,
    string Reason,
    DateTime? OccurredOn = null) : IDomainEvent;

