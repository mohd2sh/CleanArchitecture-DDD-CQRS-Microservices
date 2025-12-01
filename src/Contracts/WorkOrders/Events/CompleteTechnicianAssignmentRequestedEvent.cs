using CleanArchitecture.Core.Domain.Abstractions;

namespace CleanArchitecture.Cmms.Contracts.WorkOrders.Events;

/// <summary>
/// Published by Saga orchestrator to request completion of technician assignment.
/// Consumed by Technicians service via MassTransit message bus.
/// </summary>
public sealed record CompleteTechnicianAssignmentRequestedEvent(
    Guid TechnicianId,
    Guid WorkOrderId,
    DateTime CompletedOn,
    DateTime? OccurredOn = null) : IDomainEvent;

