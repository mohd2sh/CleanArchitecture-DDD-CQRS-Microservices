using CleanArchitecture.Core.Domain.Abstractions;

namespace CleanArchitecture.Cmms.Contracts.Technicians.Events;

/// <summary>
/// Published by Technicians service when assignment completion fails.
/// Consumed by Saga orchestrator to trigger compensation.
/// </summary>
public sealed record TechnicianAssignmentFailedEvent(
    Guid TechnicianId,
    Guid WorkOrderId,
    string ErrorMessage,
    DateTime? OccurredOn = null) : IDomainEvent;

