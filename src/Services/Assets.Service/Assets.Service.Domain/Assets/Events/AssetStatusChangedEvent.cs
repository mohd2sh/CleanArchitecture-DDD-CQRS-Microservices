using Assets.Service.Domain.Assets.Enums;
using CleanArchitecture.Core.Domain.Abstractions;

namespace Assets.Service.Domain.Assets.Events;

public sealed record AssetStatusChangedEvent(
    Guid AssetId,
    AssetStatus NewStatus,
    DateTime? OccurredOn = null
) : IDomainEvent;







