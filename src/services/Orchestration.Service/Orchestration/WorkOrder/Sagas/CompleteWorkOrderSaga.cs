using CleanArchitecture.Cmms.Contracts.Assets.Events;
using CleanArchitecture.Cmms.Contracts.Technicians.Events;
using CleanArchitecture.Cmms.Contracts.WorkOrders.Events;
using MassTransit;

namespace Orchestration.Service.WorkOrder.Sagas;

public class CompleteWorkOrderSaga : MassTransitStateMachine<CompleteWorkOrderSagaState>
{
    private readonly ILogger<CompleteWorkOrderSaga> _logger;

    public CompleteWorkOrderSaga(ILogger<CompleteWorkOrderSaga> logger)
    {
        _logger = logger;

        InstanceState(x => x.CurrentState);

        Schedule(() => CompletionTimeout, (CompleteWorkOrderSagaState saga) => saga.CompletionTimeoutTokenId, s =>
        {
            s.Received = r => r.CorrelateById(context => context.Message.WorkOrderId);
        });

        Event(() => WorkOrderCompleted, e => e.CorrelateById(context => context.Message.WorkOrderId));
        Event(() => AssetMaintenanceCompleted, e => e.CorrelateById(context => context.Message.WorkOrderId));
        Event(() => AssetMaintenanceFailed, e => e.CorrelateById(context => context.Message.WorkOrderId));
        Event(() => TechnicianAssignmentCompleted, e => e.CorrelateById(context => context.Message.WorkOrderId));
        Event(() => TechnicianAssignmentFailed, e => e.CorrelateById(context => context.Message.WorkOrderId));

        Event(() => WorkOrderCompletionRequestedFaulted, e => e.CorrelateById(context =>
        {
            return context.Message.Message.WorkOrderId;
        }));

        //Starts here when WorkOrderCompletedEvent is received
        Initially(
            When(WorkOrderCompleted)
                .Then(context =>
                {
                    _logger.LogInformation(
                        "Saga started for WorkOrder {WorkOrderId}",
                        context.Message.WorkOrderId);

                    // Initialize saga state with work order data
                    context.Saga.CorrelationId = context.Message.WorkOrderId;
                    context.Saga.WorkOrderId = context.Message.WorkOrderId;
                    context.Saga.AssetId = context.Message.AssetId;
                    context.Saga.TechnicianId = context.Message.TechnicianId;
                    context.Saga.WorkOrderCompletedAt = context.Message.OccurredOn ?? DateTime.UtcNow;

                    // Initialize completion flags
                    context.Saga.AssetCompleted = false;
                    context.Saga.TechnicianCompleted = false;

                    context.Saga.CompletionTimeoutTokenId ??= Guid.NewGuid();
                })
                // Schedule timeout: If completion doesn't finish in 30 seconds, timeout event fires
                .Schedule(CompletionTimeout,
                    context => new CompletionTimeout { WorkOrderId = context.Saga.WorkOrderId },
                    context => TimeSpan.FromSeconds(30))
                // Publish event to trigger both services in parallel
                .Publish(context => new WorkOrderCompletionRequestedEvent(
                    context.Saga.WorkOrderId,
                    context.Saga.AssetId,
                    context.Saga.TechnicianId,
                    context.Saga.WorkOrderCompletedAt!.Value))
                // Move to Processing state to wait for both services
                .TransitionTo(Processing));

        //Waiting for both Assets and Technicians services to complete
        During(Processing,
            When(AssetMaintenanceCompleted)
                .Then(context =>
                {
                    _logger.LogInformation(
                        "Asset maintenance completed for WorkOrder {WorkOrderId}",
                        context.Saga.WorkOrderId);

                    context.Saga.AssetCompleted = true;
                })
                .If(context => context.Saga.AssetCompleted && context.Saga.TechnicianCompleted,
                    then => then
                        .Unschedule(CompletionTimeout)
                        .Then(c =>
                        {
                            _logger.LogInformation(
                                "Both services completed successfully for WorkOrder {WorkOrderId}. Saga completed.",
                                c.Saga.WorkOrderId);
                        })
                        .Finalize()),

            When(TechnicianAssignmentCompleted)
                .Then(context =>
                {
                    _logger.LogInformation(
                        "Technician assignment completed for WorkOrder {WorkOrderId}",
                        context.Saga.WorkOrderId);

                    context.Saga.TechnicianCompleted = true;
                })
                .If(context => context.Saga.AssetCompleted && context.Saga.TechnicianCompleted,
                    then => then
                        .Unschedule(CompletionTimeout)
                        .Then(c =>
                        {
                            _logger.LogInformation(
                                "Both services completed successfully for WorkOrder {WorkOrderId}. Saga completed.",
                                c.Saga.WorkOrderId);
                        })
                        .Finalize()),

            When(AssetMaintenanceFailed)
                .Unschedule(CompletionTimeout)
                .Then(context =>
                {
                    var errorMessage = context.Message.ErrorMessage ?? "Asset maintenance failed";
                    _logger.LogError(
                        "Asset maintenance failed for WorkOrder {WorkOrderId}: {ErrorMessage}. Triggering compensation.",
                        context.Saga.WorkOrderId,
                        errorMessage);

                    context.Saga.ErrorMessage = errorMessage;
                })
                .Then(context =>
                {
                    // Compensation
                    _logger.LogInformation(
                        "Publishing WorkOrderCompletingFailedEvent for WorkOrder {WorkOrderId}",
                        context.Saga.WorkOrderId);

                    context.Publish(new WorkOrderCompletingFailedEvent(
                        context.Saga.WorkOrderId,
                        context.Saga.AssetId,
                        context.Saga.TechnicianId,
                        context.Saga.ErrorMessage ?? "Asset maintenance failed"));
                })
                .Finalize(),

            When(TechnicianAssignmentFailed)
                .Unschedule(CompletionTimeout)
                .Then(context =>
                {
                    var errorMessage = context.Message.ErrorMessage ?? "Technician assignment failed";
                    _logger.LogError(
                        "Technician assignment failed for WorkOrder {WorkOrderId}: {ErrorMessage}. Triggering compensation.",
                        context.Saga.WorkOrderId,
                        errorMessage);

                    context.Saga.ErrorMessage = errorMessage;
                })
                .Then(context =>
                {
                    // Compensation
                    _logger.LogInformation(
                        "Publishing WorkOrderCompletingFailedEvent for WorkOrder {WorkOrderId}",
                        context.Saga.WorkOrderId);

                    context.Publish(new WorkOrderCompletingFailedEvent(
                        context.Saga.WorkOrderId,
                        context.Saga.AssetId,
                        context.Saga.TechnicianId,
                        context.Saga.ErrorMessage ?? "Technician assignment failed"));
                })
                .Finalize(),

            When(WorkOrderCompletionRequestedFaulted)
                .Unschedule(CompletionTimeout)
                .Then(context =>
                {
                    var fault = context.Message;
                    var errorMessage = fault.Exceptions?.FirstOrDefault()?.Message ?? "Unknown error";

                    _logger.LogError(
                        "WorkOrderCompletionRequestedEvent processing failed for WorkOrder {WorkOrderId}: {ErrorMessage}. Triggering compensation.",
                        context.Saga.WorkOrderId,
                        errorMessage);

                    context.Saga.ErrorMessage = errorMessage;
                })
                .Then(context =>
                {
                    // Compensation
                    _logger.LogInformation(
                        "Publishing WorkOrderCompletingFailedEvent for WorkOrder {WorkOrderId}",
                        context.Saga.WorkOrderId);

                    context.Publish(new WorkOrderCompletingFailedEvent(
                        context.Saga.WorkOrderId,
                        context.Saga.AssetId,
                        context.Saga.TechnicianId,
                        context.Saga.ErrorMessage ?? "Handler processing failed"));
                })
                .Finalize());

        During(Processing,
            When(CompletionTimeout.Received)
                .Then(context =>
                {
                    _logger.LogWarning(
                        "Timeout: Work order completion timed out for WorkOrder {WorkOrderId}",
                        context.Saga.WorkOrderId);

                    context.Saga.ErrorMessage = "Work order completion timed out";
                })
                .Then(context =>
                {
                    // Compensation
                    _logger.LogInformation(
                        "Publishing WorkOrderCompletingFailedEvent for WorkOrder {WorkOrderId} due to timeout",
                        context.Saga.WorkOrderId);

                    context.Publish(new WorkOrderCompletingFailedEvent(
                        context.Saga.WorkOrderId,
                        context.Saga.AssetId,
                        context.Saga.TechnicianId,
                        "Work order completion timed out"));
                })
                .Finalize());
    }

    public State Processing { get; private set; } = null!;

    public Event<WorkOrderCompletedEvent> WorkOrderCompleted { get; private set; } = null!;
    public Event<AssetMaintenanceCompletedEvent> AssetMaintenanceCompleted { get; private set; } = null!;
    public Event<AssetMaintenanceFailedEvent> AssetMaintenanceFailed { get; private set; } = null!;
    public Event<TechnicianAssignmentCompletedEvent> TechnicianAssignmentCompleted { get; private set; } = null!;
    public Event<TechnicianAssignmentFailedEvent> TechnicianAssignmentFailed { get; private set; } = null!;

    public Event<Fault<WorkOrderCompletionRequestedEvent>> WorkOrderCompletionRequestedFaulted { get; private set; } = null!;

    public Schedule<CompleteWorkOrderSagaState, CompletionTimeout> CompletionTimeout { get; private set; } = null!;
}

public record CompletionTimeout
{
    public Guid WorkOrderId { get; init; }
}
