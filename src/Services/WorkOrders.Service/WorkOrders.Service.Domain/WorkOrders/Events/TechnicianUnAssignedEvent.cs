using CleanArchitecture.Core.Domain.Abstractions;

namespace WorkOrders.Service.Domain.WorkOrders.Events;

internal sealed class TechnicianUnAssignedEvent : IDomainEvent
{
    public Guid WorkOrderId { get; }
    public Guid TechnicianId { get; }
    public DateTime? OccurredOn { get; } = DateTime.UtcNow;
    public TechnicianUnAssignedEvent(Guid workOrderId, Guid technicianId)
    {
        WorkOrderId = workOrderId;
        TechnicianId = technicianId;
    }
}







