using CleanArchitecture.Core.Domain.Abstractions;

namespace WorkOrders.Service.Domain.WorkOrders.Events;

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







