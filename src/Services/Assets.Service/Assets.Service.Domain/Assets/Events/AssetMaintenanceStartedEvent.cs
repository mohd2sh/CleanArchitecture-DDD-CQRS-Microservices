using CleanArchitecture.Core.Domain.Abstractions;

namespace Assets.Service.Domain.Assets.Events;

public sealed record AssetMaintenanceStartedEvent(
    Guid AssetId,
    Guid MaintenanceRecordId,
    DateTime StartedOn,
    string Description,
    string PerformedBy,
    DateTime? OccurredOn = null) : IDomainEvent;







