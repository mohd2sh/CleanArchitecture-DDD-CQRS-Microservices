using CleanArchitecture.Cmms.Contracts.WorkOrders.Events;
using CleanArchitecture.Cmms.Domain.Technicians;
using CleanArchitecture.Core.Application.Abstractions.Events;
using CleanArchitecture.Core.Application.Abstractions.Persistence.Repositories;
using Microsoft.Extensions.Logging;

namespace CleanArchitecture.Cmms.Application.Technicians.Events.WorkOrderCompletionRequested;

/// <summary>
/// Integration event handler for WorkOrderCompletionRequestedEvent.
/// Consumes events from Saga orchestrator via MassTransit message bus.
/// Completes technician assignment - domain events are automatically published by pipeline/behavior.
/// </summary>
internal sealed class WorkOrderCompletionRequestedEventHandler
    : IIntegrationEventHandler<WorkOrderCompletionRequestedEvent>
{
    private readonly IRepository<Technician, Guid> _technicianRepository;
    private readonly ILogger<WorkOrderCompletionRequestedEventHandler> _logger;

    public WorkOrderCompletionRequestedEventHandler(
        IRepository<Technician, Guid> technicianRepository,
        ILogger<WorkOrderCompletionRequestedEventHandler> logger)
    {
        _technicianRepository = technicianRepository;
        _logger = logger;
    }

    public async Task Handle(
        WorkOrderCompletionRequestedEvent integrationEvent,
        CancellationToken cancellationToken = default)
    {
        try
        {

            _logger.LogInformation(
                "Received WorkOrderCompletionRequestedEvent for WorkOrder {WorkOrderId}, Technician {TechnicianId}",
                integrationEvent.WorkOrderId, integrationEvent.TechnicianId);

            var technician = await _technicianRepository.GetByIdAsync(integrationEvent.TechnicianId, cancellationToken);

            if (technician is null)
            {
                _logger.LogWarning(
                    "Technician with ID {TechnicianId} not found for WorkOrder {WorkOrderId}",
                    integrationEvent.TechnicianId, integrationEvent.WorkOrderId);

                throw new CleanArchitecture.Core.Application.Abstractions.Common.ApplicationException(
                    TechnicianErrors.NotFound);
            }

            technician.CompleteAssignment(integrationEvent.WorkOrderId, integrationEvent.CompletedOn);
            await _technicianRepository.UpdateAsync(technician, cancellationToken);

            // Domain event TechnicianAssignmentCompletedEvent is automatically raised by domain method
            // and published by DomainEventsIntegrationEventPipeline to outbox
            _logger.LogInformation(
                "Technician {TechnicianId} assignment completed for WorkOrder {WorkOrderId}",
                integrationEvent.TechnicianId, integrationEvent.WorkOrderId);

        }
        catch (Exception)
        {

            throw;
        }
    }
}

