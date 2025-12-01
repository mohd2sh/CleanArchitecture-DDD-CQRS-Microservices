using CleanArchitecture.Cmms.Contracts.Technicians.Events;
using CleanArchitecture.Cmms.Contracts.WorkOrders.Events;
using MassTransit;

namespace Orchestration.Service.WorkOrder.Sagas;

public class AssignTechnicianSaga : MassTransitStateMachine<AssignTechnicianSagaState>
{
    private readonly ILogger<AssignTechnicianSaga> _logger;

    public AssignTechnicianSaga(ILogger<AssignTechnicianSaga> logger)
    {
        _logger = logger;

        InstanceState(x => x.CurrentState);

        Schedule(() => ValidationTimeout, (AssignTechnicianSagaState saga) => saga.ValidationTimeoutTokenId, s =>
        {
            s.Received = r => r.CorrelateById(context => context.Message.WorkOrderId);
        });

        Event(() => WorkOrderAssigned, e => e.CorrelateById(context => context.Message.WorkOrderId));
        Event(() => TechnicianAssignmentValidated, e => e.CorrelateById(context => context.Message.WorkOrderId));
        Event(() => TechnicianAssignmentFailed, e => e.CorrelateById(context => context.Message.WorkOrderId));

        Event(() => TechnicianAssignedFaulted, e => e.CorrelateById(context =>
        {
            return context.Message.Message.WorkOrderId;
        }));

        //Initial state: starts here when WorkOrderAssignedEvent is received
        Initially(
            When(WorkOrderAssigned)
                .Then(context =>
                {
                    _logger.LogInformation(
                        "[SAGA] AssignTechnicianSaga RECEIVED WorkOrderAssignedEvent - WorkOrder {WorkOrderId}, Technician {TechnicianId}, Event Type: {EventType}",
                        context.Message.WorkOrderId,
                        context.Message.TechnicianId,
                        typeof(WorkOrderAssignedEvent).FullName);

                    context.Saga.CorrelationId = context.Message.WorkOrderId;
                    context.Saga.WorkOrderId = context.Message.WorkOrderId;
                    context.Saga.TechnicianId = context.Message.TechnicianId;
                    context.Saga.AssignedAt = context.Message.OccurredOn ?? DateTime.UtcNow;

                    context.Saga.ValidationTimeoutTokenId ??= Guid.NewGuid();
                })
                .Publish(context => new TechnicianAssignedEvent(
                    context.Saga.TechnicianId,
                    context.Saga.WorkOrderId))
                // Schedule timeout: If validation doesn't complete in 30 seconds, timeout event fires
                .Schedule(ValidationTimeout,
                    context => new ValidationTimeout { WorkOrderId = context.Saga.WorkOrderId },
                    context => TimeSpan.FromSeconds(30))
                // Move to Validating state to wait for validation result
                .TransitionTo(Validating));

        // The saga waits in this state for one of four outcomes:
        // 1. TechnicianAssignmentValidatedEvent - validation succeeded, saga completes
        // 2. Fault<TechnicianAssignedEvent> - handler threw exception, trigger compensation (automatic failure detection)
        // 3. TechnicianAssignmentFailedEvent - explicit failure event (if handler publishes it)
        // 4. ValidationTimeoutReceived - no response, assume failure, trigger compensation
        During(Validating,
            // Success Path:
            When(TechnicianAssignmentValidated)
                // Cancel the timeout since we got a response
                .Unschedule(ValidationTimeout)
                .Then(context =>
                {
                    _logger.LogInformation(
                        "Technician assignment validated successfully for WorkOrder {WorkOrderId}, Technician {TechnicianId}",
                        context.Saga.WorkOrderId,
                        context.Saga.TechnicianId);
                })
                // Finalize saga (done)
                .Finalize(),

            // Auto Failure Path: MassTransit detected consumer failure (exception thrown)
            // This is triggered automatically when TechnicianAssignedEventHandler throws an exception
            // Domain/Application exceptions: No retries, fault published immediately
            // General exceptions: Retried 3 times, then fault published if still fails
            When(TechnicianAssignedFaulted)
                // Cancel the timeout since we got a response (fault event)
                .Unschedule(ValidationTimeout)
                .Then(context =>
                {
                    var fault = context.Message;
                    var errorMessage = fault.Exceptions?.FirstOrDefault()?.Message ?? "Unknown error";

                    _logger.LogError(
                        "TechnicianAssignedEvent processing failed for WorkOrder {WorkOrderId}, Technician {TechnicianId}: {ErrorMessage}. Triggering compensation.",
                        context.Saga.WorkOrderId,
                        context.Saga.TechnicianId,
                        errorMessage);

                    context.Saga.ErrorMessage = errorMessage;
                })
                .Then(context =>
                {
                    // Compensation: Unassign technician from work order
                    _logger.LogInformation(
                        "Compensating: Unassigning Technician {TechnicianId} from WorkOrder {WorkOrderId} due to handler failure",
                        context.Saga.TechnicianId,
                        context.Saga.WorkOrderId);

                    // Publish compensation event to WorkOrders service
                    context.Publish(message: new WorkOrderAssignmentFailedEvent(
                         context.Saga.WorkOrderId,
                        context.Saga.TechnicianId
                       ));
                })
                .Finalize(),

            // EXPLICIT FAILURE PATH: Technician assignment validation failed (explicit failure event)
            When(TechnicianAssignmentFailed)
                // Cancel the timeout since we got a response
                .Unschedule(ValidationTimeout)
                .Then(context =>
                {
                    _logger.LogError(
                        "Technician assignment validation failed for WorkOrder {WorkOrderId}, Technician {TechnicianId}: {ErrorMessage}. Triggering compensation.",
                        context.Saga.WorkOrderId,
                        context.Saga.TechnicianId,
                        context.Message.ErrorMessage);

                    context.Saga.ErrorMessage = context.Message.ErrorMessage;
                })
                .Then(context =>
                {
                    // Compensation: Unassign technician from work order
                    _logger.LogInformation(
                        "Compensating: Unassigning Technician {TechnicianId} from WorkOrder {WorkOrderId}",
                        context.Saga.TechnicianId,
                        context.Saga.WorkOrderId);

                    context.Publish(message: new WorkOrderAssignmentFailedEvent(
                          context.Saga.WorkOrderId,
                         context.Saga.TechnicianId
                        ));
                })
                .Finalize());

        // TIMEOUT HANDLER: Handle scheduled timeout event
        // If validation doesn't complete within 30 seconds, assume failure and compensate
        During(Validating,
            When(ValidationTimeout.Received)
                .Then(context =>
                {
                    _logger.LogWarning(
                        "Timeout: Technician assignment validation timed out for WorkOrder {WorkOrderId}, Technician {TechnicianId}",
                        context.Saga.WorkOrderId,
                        context.Saga.TechnicianId);

                    context.Saga.ErrorMessage = "Technician assignment validation timed out";
                })
                .Then(context =>
                {
                    // COMPENSATION: Unassign technician from work order due to timeout
                    _logger.LogInformation(
                        "Compensating: Unassigning Technician {TechnicianId} from WorkOrder {WorkOrderId} due to timeout",
                        context.Saga.TechnicianId,
                        context.Saga.WorkOrderId);

                    context.Publish(message: new WorkOrderAssignmentFailedEvent(
                        context.Saga.WorkOrderId,
                       context.Saga.TechnicianId
                      ));
                })
                .Finalize());
    }

    public State Validating { get; private set; } = null!;

    public Event<WorkOrderAssignedEvent> WorkOrderAssigned { get; private set; } = null!;
    public Event<TechnicianAssignmentValidatedEvent> TechnicianAssignmentValidated { get; private set; } = null!;
    public Event<TechnicianAssignmentFailedEvent> TechnicianAssignmentFailed { get; private set; } = null!;
    public Event<Fault<TechnicianAssignedEvent>> TechnicianAssignedFaulted { get; private set; } = null!;
    public Schedule<AssignTechnicianSagaState, ValidationTimeout> ValidationTimeout { get; private set; } = null!;
}

public record ValidationTimeout
{
    public Guid WorkOrderId { get; init; }
}

