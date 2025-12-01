using CleanArchitecture.Core.Domain.Abstractions;

namespace CleanArchitecture.Cmms.Contracts.WorkOrders.Events;

/// <summary>
/// Published by Saga orchestrator when work order completion fails in either Assets or Technicians service.
/// This is a compensation event that triggers rollback in the WorkOrders service.
/// </summary>
public sealed record WorkOrderCompletingFailedEvent(
    Guid WorkOrderId,
    Guid AssetId,
    Guid TechnicianId,
    string ErrorMessage,
    DateTime? OccurredOn = null) : IDomainEvent;

