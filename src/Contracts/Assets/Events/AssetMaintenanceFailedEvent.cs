using CleanArchitecture.Core.Domain.Abstractions;

namespace CleanArchitecture.Cmms.Contracts.Assets.Events;

public sealed record AssetMaintenanceFailedEvent(
    Guid AssetId,
    Guid WorkOrderId,
    string ErrorMessage,
    DateTime? OccurredOn = null) : IDomainEvent;

