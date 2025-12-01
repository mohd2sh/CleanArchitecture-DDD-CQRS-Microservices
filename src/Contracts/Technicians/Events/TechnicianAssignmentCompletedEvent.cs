using CleanArchitecture.Core.Domain.Abstractions;

namespace CleanArchitecture.Cmms.Contracts.Technicians.Events;

/// <summary>
/// Published by Technicians service when assignment is successfully completed.
/// Consumed by Saga orchestrator to complete the saga.
/// </summary>
public sealed record TechnicianAssignmentCompletedEvent(
    Guid TechnicianId,
    Guid WorkOrderId,
    DateTime CompletedOn,
    DateTime? OccurredOn = null) : IDomainEvent;

