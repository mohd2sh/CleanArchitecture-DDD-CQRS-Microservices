using CleanArchitecture.Core.Domain.Abstractions;

namespace CleanArchitecture.Cmms.Contracts.Technicians.Events;

/// <summary>
/// Published by Technicians service when assignment validation succeeds.
/// Consumed by AssignTechnicianSaga orchestrator to complete the saga.
/// </summary>
public sealed record TechnicianAssignmentValidatedEvent(
    Guid TechnicianId,
    Guid WorkOrderId,
    DateTime? OccurredOn = null) : IDomainEvent;

