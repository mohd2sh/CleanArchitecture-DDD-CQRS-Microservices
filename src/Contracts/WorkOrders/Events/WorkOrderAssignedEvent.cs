using CleanArchitecture.Core.Domain.Abstractions;

namespace CleanArchitecture.Cmms.Contracts.WorkOrders.Events;

/// <summary>
/// Published by WorkOrders service when a technician is assigned to a work order.
/// Consumed by AssignTechnicianSaga to start orchestration.
/// </summary>
public sealed record WorkOrderAssignedEvent(
    Guid WorkOrderId,
    Guid TechnicianId,
    DateTime? OccurredOn = null) : IDomainEvent;


