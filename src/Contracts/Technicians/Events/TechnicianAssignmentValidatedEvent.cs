using CleanArchitecture.Core.Domain.Abstractions;

namespace CleanArchitecture.Cmms.Contracts.Technicians.Events;

public sealed record TechnicianAssignmentValidatedEvent(
    Guid TechnicianId,
    Guid WorkOrderId,
    DateTime? OccurredOn = null) : IDomainEvent;

