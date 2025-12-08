using CleanArchitecture.Core.Domain.Abstractions;

namespace CleanArchitecture.Cmms.Contracts.WorkOrders.Events;

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

