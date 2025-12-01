using CleanArchitecture.Core.Domain.Abstractions;

namespace CleanArchitecture.Cmms.Contracts.WorkOrders.Events;

/// <summary>
/// Published by WorkOrders service when a technician is assigned to a work order.
/// Consumed by Technicians service to track assignments.
/// </summary>
public sealed class TechnicianAssignedEvent : IDomainEvent
{
    public Guid TechnicianId { get; }
    public Guid WorkOrderId { get; }
    public DateTime? OccurredOn { get; } = DateTime.UtcNow;

    public TechnicianAssignedEvent(Guid technicianId, Guid workOrderId)
    {
        TechnicianId = technicianId;
        WorkOrderId = workOrderId;
    }
}

