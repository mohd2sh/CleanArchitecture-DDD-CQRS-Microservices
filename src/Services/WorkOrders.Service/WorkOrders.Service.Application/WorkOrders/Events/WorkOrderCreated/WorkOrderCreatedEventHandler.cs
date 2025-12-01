using CleanArchitecture.Core.Application.Abstractions.Events;
using Microsoft.Extensions.Logging;
using WorkOrders.Service.Domain.WorkOrders.Events;

namespace WorkOrders.Service.Application.WorkOrders.Events.WorkOrderCreated;

public class WorkOrderCreatedEventHandler : IDomainEventHandler<WorkOrderCreatedEvent>
{
    private readonly ILogger<WorkOrderCreatedEventHandler> _logger;

    public WorkOrderCreatedEventHandler(ILogger<WorkOrderCreatedEventHandler> logger)
    {
        _logger = logger;
    }

    public Task Handle(WorkOrderCreatedEvent domainEvent, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Work Order Created Event Handled: {WorkOrderId}", domainEvent.WorkOrderId);

        return Task.CompletedTask;
    }
}


