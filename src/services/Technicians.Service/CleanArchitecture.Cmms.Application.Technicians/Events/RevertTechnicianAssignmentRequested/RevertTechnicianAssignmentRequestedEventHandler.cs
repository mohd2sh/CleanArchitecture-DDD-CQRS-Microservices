using CleanArchitecture.Cmms.Contracts.Technicians.Events;
using CleanArchitecture.Cmms.Domain.Technicians;
using CleanArchitecture.Core.Application.Abstractions.Events;
using CleanArchitecture.Core.Application.Abstractions.Persistence.Repositories;
using Microsoft.Extensions.Logging;

namespace CleanArchitecture.Cmms.Application.Technicians.Events.RevertTechnicianAssignmentRequested;

/// <summary>
/// Integration event handler for RevertTechnicianAssignmentRequestedEvent.
/// Consumes events from Saga orchestrator via MassTransit message bus.
/// Reverts technician assignment when compensation is needed.
/// </summary>
internal sealed class RevertTechnicianAssignmentRequestedEventHandler : IIntegrationEventHandler<RevertTechnicianAssignmentRequestedEvent>
{
    private readonly IRepository<Technician, Guid> _technicianRepository;
    private readonly ILogger<RevertTechnicianAssignmentRequestedEventHandler> _logger;

    public RevertTechnicianAssignmentRequestedEventHandler(
        IRepository<Technician, Guid> technicianRepository,
        ILogger<RevertTechnicianAssignmentRequestedEventHandler> logger)
    {
        _technicianRepository = technicianRepository;
        _logger = logger;
    }

    public async Task Handle(RevertTechnicianAssignmentRequestedEvent integrationEvent, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Received RevertTechnicianAssignmentRequestedEvent for Technician {TechnicianId}, WorkOrder {WorkOrderId}. Reason: {Reason}",
            integrationEvent.TechnicianId, integrationEvent.WorkOrderId, integrationEvent.Reason);

        var technician = await _technicianRepository.GetByIdAsync(integrationEvent.TechnicianId, cancellationToken);

        if (technician is null)
        {
            _logger.LogWarning(
                "Technician with ID {TechnicianId} not found for WorkOrder {WorkOrderId} during compensation",
                integrationEvent.TechnicianId, integrationEvent.WorkOrderId);

            throw new CleanArchitecture.Core.Application.Abstractions.Common.ApplicationException(TechnicianErrors.NotFound);
        }

        technician.UnAssignedOrder(integrationEvent.WorkOrderId);

        await _technicianRepository.UpdateAsync(technician, cancellationToken);

        _logger.LogInformation(
            "Technician {TechnicianId} assignment reverted for WorkOrder {WorkOrderId}",
            integrationEvent.TechnicianId, integrationEvent.WorkOrderId);
    }
}

