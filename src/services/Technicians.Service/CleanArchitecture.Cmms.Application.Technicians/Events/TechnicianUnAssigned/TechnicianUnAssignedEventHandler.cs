using CleanArchitecture.Cmms.Contracts.WorkOrders.Events;
using CleanArchitecture.Cmms.Domain.Technicians;
using CleanArchitecture.Core.Application.Abstractions.Events;
using CleanArchitecture.Core.Application.Abstractions.Persistence.Repositories;
using Microsoft.Extensions.Logging;

namespace CleanArchitecture.Cmms.Application.Technicians.Events.TechnicianUnAssigned;

/// <summary>
/// Integration event handler for TechnicianUnAssignedEvent.
/// Consumes events from WorkOrders service via MassTransit message bus.
/// Removes assignment from technician when unassigned from a work order.
/// </summary>
internal class TechnicianUnAssignedEventHandler : IIntegrationEventHandler<TechnicianUnAssignedEvent>
{
    private readonly IRepository<Technician, Guid> _technicianRepository;
    private readonly ILogger<TechnicianUnAssignedEventHandler> _logger;

    public TechnicianUnAssignedEventHandler(
        IRepository<Technician, Guid> technicianRepository,
        ILogger<TechnicianUnAssignedEventHandler> logger)
    {
        _technicianRepository = technicianRepository;
        _logger = logger;
    }

    public async Task Handle(TechnicianUnAssignedEvent integrationEvent, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Received TechnicianUnAssignedEvent for Technician {TechnicianId}, WorkOrder {WorkOrderId}",
            integrationEvent.TechnicianId, integrationEvent.WorkOrderId);

        var technician = await _technicianRepository.GetByIdAsync(integrationEvent.TechnicianId, cancellationToken);

        if (technician == null)
        {
            _logger.LogWarning(
                "Technician with ID {TechnicianId} not found for WorkOrder {WorkOrderId}",
                integrationEvent.TechnicianId, integrationEvent.WorkOrderId);

            throw new CleanArchitecture.Core.Application.Abstractions.Common.ApplicationException(TechnicianErrors.NotFound);
        }

        technician.UnAssignedOrder(integrationEvent.WorkOrderId);

        await _technicianRepository.UpdateAsync(technician, cancellationToken);

        _logger.LogInformation(
            "Technician {TechnicianId} unassigned from WorkOrder {WorkOrderId}",
            integrationEvent.TechnicianId, integrationEvent.WorkOrderId);
    }
}

