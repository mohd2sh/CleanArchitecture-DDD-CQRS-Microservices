using CleanArchitecture.Core.Domain.Abstractions;

namespace Assets.Service.Domain.Assets.Events;

public sealed record AssetLocationUpdatedEvent(
  Guid AssetId,
  DateTime? OccurredOn = null
) : IDomainEvent;







