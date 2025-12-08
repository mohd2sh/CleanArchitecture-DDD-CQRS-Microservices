using CleanArchitecture.Core.Domain.Abstractions;

namespace CleanArchitecture.Cmms.Contracts.Assets.Events;

public sealed record AssetMaintenanceCompletedEvent(
    Guid AssetId,
    Guid WorkOrderId,
    DateTime CompletedOn,
    DateTime? OccurredOn = null) : IDomainEvent;

