using CleanArchitecture.Core.Domain.Abstractions;

namespace CleanArchitecture.Cmms.Contracts.Assets.Events;

/// <summary>
/// Published by Assets service when maintenance is successfully completed.
/// Consumed by Saga orchestrator to proceed to next step.
/// </summary>
public sealed record AssetMaintenanceCompletedEvent(
    Guid AssetId,
    Guid WorkOrderId,
    DateTime CompletedOn,
    DateTime? OccurredOn = null) : IDomainEvent;

