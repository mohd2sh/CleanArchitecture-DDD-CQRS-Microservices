using CleanArchitecture.Core.Domain.Abstractions;

namespace CleanArchitecture.Cmms.Contracts.WorkOrders.Events;

/// <summary>
/// Published by Saga service when a technician assignment failed as COMPENSATION events or Rollback.
/// </summary>
public sealed record WorkOrderAssignmentFailedEvent(
    Guid WorkOrderId,
    Guid TechnicianId,
    DateTime? OccurredOn = null) : IDomainEvent;

