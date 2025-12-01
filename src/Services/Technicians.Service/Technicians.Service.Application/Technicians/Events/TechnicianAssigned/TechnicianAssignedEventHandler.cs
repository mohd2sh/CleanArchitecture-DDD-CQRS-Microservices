using CleanArchitecture.Cmms.Contracts.WorkOrders.Events;
using CleanArchitecture.Core.Application.Abstractions.Events;
using CleanArchitecture.Core.Application.Abstractions.Persistence.Repositories;
using Microsoft.Extensions.Logging;
using Technicians.Service.Domain.Technicians;

namespace Technicians.Service.Application.Technicians.Events.TechnicianAssigned;

/// <summary>
/// Integration event handler for TechnicianAssignedEvent.
/// Consumes events from AssignTechnicianSaga (Orchestration service) via MassTransit message bus.
/// Adds assignment to technician when assigned to a work order.
/// </summary>
internal class TechnicianAssignedEventHandler : IIntegrationEventHandler<TechnicianAssignedEvent>
{
    private readonly IRepository<Technician, Guid> _technicianRepository;
    private readonly ILogger<TechnicianAssignedEventHandler> _logger;

    public TechnicianAssignedEventHandler(
        IRepository<Technician, Guid> technicianRepository,
        ILogger<TechnicianAssignedEventHandler> logger)
    {
        _technicianRepository = technicianRepository;
        _logger = logger;
    }

    public async Task Handle(TechnicianAssignedEvent integrationEvent, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Received TechnicianAssignedEvent for Technician {TechnicianId}, WorkOrder {WorkOrderId}",
            integrationEvent.TechnicianId, integrationEvent.WorkOrderId);
        try
        {

            var technician = await _technicianRepository.GetByIdAsync(integrationEvent.TechnicianId, cancellationToken);

            if (technician == null)
            {
                _logger.LogWarning(
                    "Technician with ID {TechnicianId} not found for WorkOrder {WorkOrderId}",
                    integrationEvent.TechnicianId, integrationEvent.WorkOrderId);

                // Throw exception - transaction will roll back
                throw new CleanArchitecture.Core.Application.Abstractions.Common.ApplicationException(TechnicianErrors.NotFound);
            }

            // Domain will raise TechnicianAssignmentValidatedEvent when AddAssignedOrder is called
            technician.AddAssignedOrder(integrationEvent.WorkOrderId, integrationEvent.OccurredOn ?? DateTime.UtcNow);

            await _technicianRepository.UpdateAsync(technician, cancellationToken);

            _logger.LogInformation(
                "Technician {TechnicianId} assigned to WorkOrder {WorkOrderId}",
                integrationEvent.TechnicianId, integrationEvent.WorkOrderId);


        }
        catch (Exception ex)
        {

            throw;
        }
    }
}


