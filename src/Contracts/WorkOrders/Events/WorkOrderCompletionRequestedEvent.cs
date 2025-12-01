using CleanArchitecture.Core.Domain.Abstractions;

namespace CleanArchitecture.Cmms.Contracts.WorkOrders.Events;

/// <summary>
/// Published by Saga orchestrator to request completion processing from Assets and Technicians services.
/// This event is published after the saga receives WorkOrderCompletedEvent from WorkOrders service.
/// </summary>
public sealed record WorkOrderCompletionRequestedEvent(
    Guid WorkOrderId,
    Guid AssetId,
    Guid TechnicianId,
    DateTime CompletedOn,
    DateTime? OccurredOn = null) : IDomainEvent;

