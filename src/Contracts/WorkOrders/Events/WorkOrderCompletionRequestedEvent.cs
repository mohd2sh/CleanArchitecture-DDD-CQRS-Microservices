using CleanArchitecture.Core.Domain.Abstractions;

namespace CleanArchitecture.Cmms.Contracts.WorkOrders.Events;

public sealed record WorkOrderCompletionRequestedEvent(
    Guid WorkOrderId,
    Guid AssetId,
    Guid TechnicianId,
    DateTime CompletedOn,
    DateTime? OccurredOn = null) : IDomainEvent;

