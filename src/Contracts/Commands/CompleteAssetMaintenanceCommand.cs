namespace CleanArchitecture.Cmms.Contracts.Commands;

/// <summary>
/// Command sent by Saga orchestrator to Assets service to complete maintenance.
/// This is a point-to-point command (single consumer).
/// </summary>
public sealed record CompleteAssetMaintenanceCommand(
    Guid AssetId,
    Guid WorkOrderId,
    DateTime CompletedOn,
    string Notes);


