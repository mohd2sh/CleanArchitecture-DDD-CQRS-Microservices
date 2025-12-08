using CleanArchitecture.Core.Domain.Abstractions;

namespace CleanArchitecture.Cmms.Contracts.Technicians.Events;

public sealed record TechnicianAssignmentFailedEvent(
    Guid TechnicianId,
    Guid WorkOrderId,
    string ErrorMessage,
    DateTime? OccurredOn = null) : IDomainEvent;

