using CleanArchitecture.Core.Domain.Abstractions;

namespace CleanArchitecture.Cmms.Domain.Assets.Events;

public sealed record AssetMaintenanceCompletedEvent(
    Guid AssetId,
    DateTime CompletedOn,
    string Notes,
    Guid? WorkOrderId = null,
    DateTime? OccurredOn = null
    ) : IDomainEvent;

