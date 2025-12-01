namespace Technicians.Service.Application.Technicians.Dtos;

public sealed class TechnicianAssignmentDto
{
    public Guid WorkOrderId { get; init; }
    public DateTime AssignedOn { get; init; }
    public DateTime? CompletedOn { get; init; }
    public bool IsCompleted => CompletedOn != null;
}


