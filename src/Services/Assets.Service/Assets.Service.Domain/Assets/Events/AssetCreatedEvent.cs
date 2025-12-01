using CleanArchitecture.Core.Domain.Abstractions;

namespace Assets.Service.Domain.Assets.Events;

public sealed record AssetCreatedEvent(
 Guid AssetId,
 string Name,
 string Type,
 string TagValue,
 DateTime? OccurredOn = null) : IDomainEvent;







