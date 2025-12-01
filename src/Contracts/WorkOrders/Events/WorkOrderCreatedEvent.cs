using CleanArchitecture.Core.Domain.Abstractions;

namespace CleanArchitecture.Cmms.Contracts.WorkOrders.Events;

/// <summary>
/// Published by WorkOrders service when a work order is created.
/// Consumed by Assets service to set asset under maintenance.
/// </summary>
public sealed class WorkOrderCreatedEvent : IDomainEvent
{
    public Guid WorkOrderId { get; }
    public string Title { get; }
    public Guid AssetId { get; }
    public DateTime? OccurredOn { get; } = DateTime.UtcNow;

    public WorkOrderCreatedEvent(Guid workOrderId, Guid assetId, string title)
    {
        WorkOrderId = workOrderId;
        AssetId = assetId;
        Title = title;
    }
}

