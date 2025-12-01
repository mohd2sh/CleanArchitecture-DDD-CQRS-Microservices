namespace CleanArchitecture.Cmms.Contracts.Commands;

/// <summary>
/// Compensation command sent by Saga orchestrator to Technicians service to revert assignment completion.
/// Used when saga needs to rollback due to failure in subsequent steps.
/// </summary>
public sealed record RevertTechnicianAssignmentCommand(
    Guid TechnicianId,
    Guid WorkOrderId,
    string Reason);


