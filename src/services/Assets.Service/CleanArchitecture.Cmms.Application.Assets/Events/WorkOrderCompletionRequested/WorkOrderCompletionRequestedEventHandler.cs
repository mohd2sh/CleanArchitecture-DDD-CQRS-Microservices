using CleanArchitecture.Cmms.Contracts.WorkOrders.Events;
using CleanArchitecture.Cmms.Domain.Assets;
using CleanArchitecture.Core.Application.Abstractions.Common;
using CleanArchitecture.Core.Application.Abstractions.Events;
using CleanArchitecture.Core.Application.Abstractions.Persistence.Repositories;
using Microsoft.Extensions.Logging;

namespace CleanArchitecture.Cmms.Application.Assets.Events.WorkOrderCompletionRequested;

/// <summary>
/// Integration event handler for WorkOrderCompletionRequestedEvent.
/// Consumes events from Saga orchestrator via MassTransit message bus.
/// Completes asset maintenance - domain events are automatically published by pipeline/behavior.
/// </summary>
internal sealed class WorkOrderCompletionRequestedEventHandler
    : IIntegrationEventHandler<WorkOrderCompletionRequestedEvent>
{
    private readonly IRepository<Asset, Guid> _assetRepository;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly ILogger<WorkOrderCompletionRequestedEventHandler> _logger;

    public WorkOrderCompletionRequestedEventHandler(
        IRepository<Asset, Guid> assetRepository,
        IDateTimeProvider dateTimeProvider,
        ILogger<WorkOrderCompletionRequestedEventHandler> logger)
    {
        _assetRepository = assetRepository;
        _dateTimeProvider = dateTimeProvider;
        _logger = logger;
    }

    public async Task Handle(
        WorkOrderCompletionRequestedEvent integrationEvent,
        CancellationToken cancellationToken = default)
    {
        try
        {

            _logger.LogInformation(
                "Received WorkOrderCompletionRequestedEvent for WorkOrder {WorkOrderId}, Asset {AssetId}",
                integrationEvent.WorkOrderId, integrationEvent.AssetId);

            var asset = await _assetRepository.GetByIdAsync(integrationEvent.AssetId, cancellationToken);
            if (asset is null)
            {
                _logger.LogWarning(
                    "Asset with ID {AssetId} not found for Work Order {WorkOrderId}",
                    integrationEvent.AssetId, integrationEvent.WorkOrderId);

                throw new CleanArchitecture.Core.Application.Abstractions.Common.ApplicationException(Assets.AssetErrors.NotFound);
            }

            asset.CompleteMaintenance(integrationEvent.CompletedOn);
            await _assetRepository.UpdateAsync(asset, cancellationToken);

            // Domain event AssetMaintenanceCompletedEvent is automatically raised by domain method
            // and published by DomainEventsIntegrationEventPipeline to outbox
            _logger.LogInformation(
                "Asset {AssetId} maintenance completed for WorkOrder {WorkOrderId}",
                integrationEvent.AssetId, integrationEvent.WorkOrderId);

        }
        catch (Exception)
        {

            throw;
        }
    }
}

