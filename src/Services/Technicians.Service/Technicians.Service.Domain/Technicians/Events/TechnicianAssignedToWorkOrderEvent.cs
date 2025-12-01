using CleanArchitecture.Core.Domain.Abstractions;

namespace Technicians.Service.Domain.Technicians.Events;

public sealed record TechnicianAssignedToWorkOrderEvent(Guid TechnicianId, Guid WorkOrderId, DateTime? OccurredOn = null) : IDomainEvent;


