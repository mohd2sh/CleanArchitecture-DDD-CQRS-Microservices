using CleanArchitecture.Core.Domain.Abstractions;

namespace CleanArchitecture.Cmms.Contracts.WorkOrders.Events;

public sealed record WorkOrderAssignedEvent(
    Guid WorkOrderId,
    Guid TechnicianId,
    DateTime? OccurredOn = null) : IDomainEvent;

