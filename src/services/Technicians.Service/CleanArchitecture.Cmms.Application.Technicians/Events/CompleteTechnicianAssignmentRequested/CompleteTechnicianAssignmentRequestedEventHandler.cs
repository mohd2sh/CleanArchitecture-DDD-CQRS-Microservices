using CleanArchitecture.Cmms.Contracts.WorkOrders.Events;
using CleanArchitecture.Cmms.Domain.Technicians;
using CleanArchitecture.Core.Application.Abstractions.Events;
using CleanArchitecture.Core.Application.Abstractions.Persistence.Repositories;
using Microsoft.Extensions.Logging;

namespace CleanArchitecture.Cmms.Application.Technicians.Events.CompleteTechnicianAssignmentRequested;

/// <summary>
/// Integration event handler for CompleteTechnicianAssignmentRequestedEvent.
/// Consumes events from Saga orchestrator via MassTransit message bus.
/// Completes technician assignment when requested by saga.
/// </summary>
internal sealed class CompleteTechnicianAssignmentRequestedEventHandler : IIntegrationEventHandler<CompleteTechnicianAssignmentRequestedEvent>
{
    private readonly IRepository<Technician, Guid> _technicianRepository;
    private readonly ILogger<CompleteTechnicianAssignmentRequestedEventHandler> _logger;

    public CompleteTechnicianAssignmentRequestedEventHandler(
        IRepository<Technician, Guid> technicianRepository,
        ILogger<CompleteTechnicianAssignmentRequestedEventHandler> logger)
    {
        _technicianRepository = technicianRepository;
        _logger = logger;
    }

    public async Task Handle(CompleteTechnicianAssignmentRequestedEvent integrationEvent, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Received CompleteTechnicianAssignmentRequestedEvent for Technician {TechnicianId}, WorkOrder {WorkOrderId}",
            integrationEvent.TechnicianId, integrationEvent.WorkOrderId);

        var technician = await _technicianRepository.GetByIdAsync(integrationEvent.TechnicianId, cancellationToken);

        if (technician is null)
        {
            _logger.LogWarning(
                "Technician with ID {TechnicianId} not found for WorkOrder {WorkOrderId}",
                integrationEvent.TechnicianId, integrationEvent.WorkOrderId);

            throw new CleanArchitecture.Core.Application.Abstractions.Common.ApplicationException(TechnicianErrors.NotFound);
        }

        technician.CompleteAssignment(integrationEvent.WorkOrderId, integrationEvent.CompletedOn);

        await _technicianRepository.UpdateAsync(technician, cancellationToken);

        _logger.LogInformation(
            "Technician {TechnicianId} assignment completed for WorkOrder {WorkOrderId}",
            integrationEvent.TechnicianId, integrationEvent.WorkOrderId);
    }
}

