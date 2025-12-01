using CleanArchitecture.Core.Domain.Abstractions;

namespace Assets.Service.Domain.Assets.Entities;

internal sealed class MaintenanceRecord : Entity<Guid>
{
    public Guid AssetId { get; private set; }
    public Guid WorkOrderId { get; private set; }
    public DateTime StartedOn { get; private set; }
    public string Description { get; private set; }
    public string PerformedBy { get; private set; }
    public bool IsCompleted { get; private set; }

    private MaintenanceRecord() { } // For EF

    private MaintenanceRecord(Guid assetId, Guid workOrderId, DateTime startedOn, string description, string performedBy)
        : base(Guid.NewGuid())
    {
        if (string.IsNullOrWhiteSpace(description))
            throw new DomainException(AssetErrors.MaintenanceDescriptionRequired);

        if (string.IsNullOrWhiteSpace(performedBy))
            throw new DomainException(AssetErrors.MaintenancePerformerRequired);

        AssetId = assetId;
        WorkOrderId = workOrderId;
        StartedOn = startedOn;
        Description = description.Trim();
        PerformedBy = performedBy.Trim();
    }

    public void Complete()
    {
        IsCompleted = true;
    }

    public static MaintenanceRecord Create(Guid assetId, Guid workOrderId, DateTime startedOn, string description, string performedBy)
        => new(assetId, workOrderId, startedOn, description, performedBy);
}







