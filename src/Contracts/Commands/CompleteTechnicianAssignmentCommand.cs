namespace CleanArchitecture.Cmms.Contracts.Commands;

/// <summary>
/// Command sent by Saga orchestrator to Technicians service to complete assignment.
/// This is a point-to-point command (single consumer).
/// </summary>
public sealed record CompleteTechnicianAssignmentCommand(
    Guid TechnicianId,
    Guid WorkOrderId,
    DateTime CompletedOn);


