using CleanArchitecture.Core.Domain.Abstractions;

namespace CleanArchitecture.Cmms.Contracts.Technicians.Events;

public sealed record TechnicianAssignmentCompletedEvent(
    Guid TechnicianId,
    Guid WorkOrderId,
    DateTime CompletedOn,
    DateTime? OccurredOn = null) : IDomainEvent;

