using MassTransit;

namespace Orchestration.Service.WorkOrder.Sagas;

public class AssignTechnicianSagaState : SagaStateMachineInstance
{
    public Guid CorrelationId { get; set; }
    public string CurrentState { get; set; } = string.Empty;

    public Guid WorkOrderId { get; set; }
    public Guid TechnicianId { get; set; }
    public DateTime? AssignedAt { get; set; }

    public string? ErrorMessage { get; set; }

    public Guid? ValidationTimeoutTokenId { get; set; }
}

