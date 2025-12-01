using MassTransit;

namespace Orchestration.Service.WorkOrder.Sagas;

public class CompleteWorkOrderSagaState : SagaStateMachineInstance
{
    public Guid CorrelationId { get; set; }
    public string CurrentState { get; set; } = string.Empty;

    public Guid WorkOrderId { get; set; }
    public Guid AssetId { get; set; }
    public Guid TechnicianId { get; set; }
    public DateTime? WorkOrderCompletedAt { get; set; }

    public bool AssetCompleted { get; set; }
    public bool TechnicianCompleted { get; set; }

    public string? ErrorMessage { get; set; }

    public Guid? CompletionTimeoutTokenId { get; set; }
}

