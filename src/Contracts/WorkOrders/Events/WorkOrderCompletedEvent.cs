using CleanArchitecture.Core.Domain.Abstractions;

namespace CleanArchitecture.Cmms.Contracts.WorkOrders.Events;

/// <summary>
/// Published by WorkOrders service when a work order is completed.
/// Consumed by Assets service to complete maintenance and Technicians service to complete assignment.
/// </summary>
public sealed class WorkOrderCompletedEvent : IDomainEvent
{
    public Guid WorkOrderId { get; }
    public Guid AssetId { get; }
    public Guid TechnicianId { get; }
    public DateTime? OccurredOn { get; } = DateTime.UtcNow;

    public WorkOrderCompletedEvent(Guid workOrderId, Guid assetId, Guid technicianId)
    {
        WorkOrderId = workOrderId;
        AssetId = assetId;
        TechnicianId = technicianId;
    }
}

