using CleanArchitecture.Cmms.Contracts.WorkOrders.Events;
using CleanArchitecture.Cmms.Domain.Assets;
using CleanArchitecture.Core.Application.Abstractions.Common;
using CleanArchitecture.Core.Application.Abstractions.Events;
using CleanArchitecture.Core.Application.Abstractions.Persistence.Repositories;
using Microsoft.Extensions.Logging;

namespace CleanArchitecture.Cmms.Application.Assets.Events.WorkOrderCreated;

/// <summary>
/// Integration event handler for WorkOrderCreatedEvent.
/// Consumes events from WorkOrders service via MassTransit message bus.
/// Sets asset under maintenance when a work order is created.
/// </summary>
internal sealed class WorkOrderCreatedEventHandler
: IIntegrationEventHandler<WorkOrderCreatedEvent>
{
    private readonly IRepository<Asset, Guid> _assetRepository;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly ILogger<WorkOrderCreatedEventHandler> _logger;

    public WorkOrderCreatedEventHandler(
        IRepository<Asset, Guid> assetRepository,
        IDateTimeProvider dateTimeProvider,
        ILogger<WorkOrderCreatedEventHandler> logger)
    {
        _assetRepository = assetRepository;
        _dateTimeProvider = dateTimeProvider;
        _logger = logger;
    }

    public async Task Handle(
        WorkOrderCreatedEvent integrationEvent,
        CancellationToken cancellationToken = default)
    {
        try
        {

            _logger.LogInformation(
                "Received WorkOrderCreatedEvent for WorkOrder {WorkOrderId}, Asset {AssetId}",
                integrationEvent.WorkOrderId, integrationEvent.AssetId);

            var asset = await _assetRepository.GetByIdAsync(integrationEvent.AssetId, cancellationToken);

            if (asset is null)
            {
                _logger.LogWarning(
                    "Asset with ID {AssetId} not found for Work Order {WorkOrderId}",
                    integrationEvent.AssetId, integrationEvent.WorkOrderId);

                throw new CleanArchitecture.Core.Application.Abstractions.Common.ApplicationException(Assets.AssetErrors.NotFound);
            }

            asset.SetUnderMaintenance(
                workOrderId: integrationEvent.WorkOrderId,
                description: $"Work order '{integrationEvent.Title}' created",
                performedBy: "System",
                startedOn: _dateTimeProvider.UtcNow);

            await _assetRepository.UpdateAsync(asset, cancellationToken);

            _logger.LogInformation(
                "Asset {AssetId} set under maintenance for WorkOrder {WorkOrderId}",
                integrationEvent.AssetId, integrationEvent.WorkOrderId);
        }
        catch (Exception)
        {

            throw;
        }
    }
}

