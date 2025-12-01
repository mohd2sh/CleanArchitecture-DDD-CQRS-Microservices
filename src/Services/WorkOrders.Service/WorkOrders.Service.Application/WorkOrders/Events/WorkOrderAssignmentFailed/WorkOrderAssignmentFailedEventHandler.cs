using CleanArchitecture.Cmms.Contracts.WorkOrders.Events;
using CleanArchitecture.Core.Application.Abstractions.Events;
using CleanArchitecture.Core.Application.Abstractions.Persistence.Repositories;
using Microsoft.Extensions.Logging;
using WorkOrders.Service.Domain.WorkOrders;

namespace WorkOrders.Service.Application.WorkOrders.Events.WorkOrderAssignmentFailed;
internal class TechnicianAssignedEventHandler : IIntegrationEventHandler<WorkOrderAssignmentFailedEvent>
{
    private readonly IRepository<WorkOrder, Guid> _workOrderRepository;
    private readonly ILogger<TechnicianAssignedEventHandler> _logger;

    public TechnicianAssignedEventHandler(
        IRepository<WorkOrder, Guid> technicianRepository,
        ILogger<TechnicianAssignedEventHandler> logger)
    {
        _workOrderRepository = technicianRepository;
        _logger = logger;
    }

    public async Task Handle(WorkOrderAssignmentFailedEvent integrationEvent, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("UnAssigning WorkOrder: {WorkOrderId}", integrationEvent.WorkOrderId);

        var workOrder = await _workOrderRepository.GetByIdAsync(integrationEvent.WorkOrderId, cancellationToken);

        if (workOrder == null)
        {
            // Throw exception - transaction will roll back
            throw new CleanArchitecture.Core.Application.Abstractions.Common.ApplicationException(WorkOrderErrors.NotFound);
        }

        workOrder.UnAssignTechnician();

        await _workOrderRepository.UpdateAsync(workOrder, cancellationToken);

        _logger.LogInformation(
            "WorkOrder {WorkOrderId} was un-assigned",
            integrationEvent.WorkOrderId);



    }
}
