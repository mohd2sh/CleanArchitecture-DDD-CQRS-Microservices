using CleanArchitecture.Core.Domain.Abstractions;

namespace CleanArchitecture.Cmms.Contracts.Assets.Events;

/// <summary>
/// Published by Assets service when maintenance completion fails.
/// Consumed by Saga orchestrator to trigger compensation.
/// </summary>
public sealed record AssetMaintenanceFailedEvent(
    Guid AssetId,
    Guid WorkOrderId,
    string ErrorMessage,
    DateTime? OccurredOn = null) : IDomainEvent;

