using CleanArchitecture.Cmms.Contracts.WorkOrders.Events;
using CleanArchitecture.Cmms.Domain.WorkOrders;
using CleanArchitecture.Core.Application.Abstractions.Events;
using CleanArchitecture.Core.Application.Abstractions.Persistence.Repositories;
using Microsoft.Extensions.Logging;

namespace CleanArchitecture.Cmms.Application.WorkOrders.Events.WorkOrderCompletingFailed;

/// <summary>
/// Integration event handler for WorkOrderCompletingFailedEvent.
/// Consumes compensation events from Saga orchestrator via MassTransit message bus.
/// Reverts work order status from Completed back to InProgress when saga compensation is triggered.
/// </summary>
internal sealed class WorkOrderCompletingFailedEventHandler
    : IIntegrationEventHandler<WorkOrderCompletingFailedEvent>
{
    private readonly IRepository<WorkOrder, Guid> _workOrderRepository;
    private readonly ILogger<WorkOrderCompletingFailedEventHandler> _logger;

    public WorkOrderCompletingFailedEventHandler(
        IRepository<WorkOrder, Guid> workOrderRepository,
        ILogger<WorkOrderCompletingFailedEventHandler> logger)
    {
        _workOrderRepository = workOrderRepository;
        _logger = logger;
    }

    public async Task Handle(
        WorkOrderCompletingFailedEvent integrationEvent,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Received WorkOrderCompletingFailedEvent for WorkOrder {WorkOrderId}: {ErrorMessage}",
            integrationEvent.WorkOrderId, integrationEvent.ErrorMessage);

        var workOrder = await _workOrderRepository.GetByIdAsync(integrationEvent.WorkOrderId, cancellationToken);

        if (workOrder is null)
        {
            _logger.LogWarning(
                "WorkOrder with ID {WorkOrderId} not found for compensation",
                integrationEvent.WorkOrderId);
            return;
        }

        try
        {
            workOrder.RevertCompletion();
            await _workOrderRepository.UpdateAsync(workOrder, cancellationToken);

            _logger.LogInformation(
                "WorkOrder {WorkOrderId} status reverted from Completed to InProgress due to: {ErrorMessage}",
                integrationEvent.WorkOrderId, integrationEvent.ErrorMessage);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to revert WorkOrder {WorkOrderId} completion status",
                integrationEvent.WorkOrderId);
            throw;
        }
    }
}

