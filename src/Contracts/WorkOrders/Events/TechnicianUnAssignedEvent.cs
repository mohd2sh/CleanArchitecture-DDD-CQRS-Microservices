using CleanArchitecture.Core.Domain.Abstractions;

namespace CleanArchitecture.Cmms.Contracts.WorkOrders.Events;

/// <summary>
/// Published by WorkOrders service when a technician is unassigned from a work order.
/// Consumed by Technicians service to remove assignments.
/// </summary>
public sealed class TechnicianUnAssignedEvent : IDomainEvent
{
    public Guid TechnicianId { get; }
    public Guid WorkOrderId { get; }
    public DateTime? OccurredOn { get; } = DateTime.UtcNow;

    public TechnicianUnAssignedEvent(Guid technicianId, Guid workOrderId)
    {
        TechnicianId = technicianId;
        WorkOrderId = workOrderId;
    }
}

