using Technicians.Service.Domain.Technicians.Enums;
using CleanArchitecture.Core.Domain.Abstractions;

namespace Technicians.Service.Domain.Technicians.Events;

public sealed record TechnicianStatusChangedEvent(Guid TechnicianId, TechnicianStatus NewStatus, DateTime? OccurredOn = null) : IDomainEvent;


