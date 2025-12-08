using CleanArchitecture.Core.Domain.Abstractions;

namespace CleanArchitecture.Cmms.Contracts.Technicians.Events;

public sealed record RevertTechnicianAssignmentRequestedEvent(
    Guid TechnicianId,
    Guid WorkOrderId,
    string Reason,
    DateTime? OccurredOn = null) : IDomainEvent;

