using CleanArchitecture.Core.Domain.Abstractions;

namespace Technicians.Service.Domain.Technicians.Events;

public sealed record TechnicianCreatedEvent(Guid TechnicianId, string Name, string SkillLevelName, DateTime? OccurredOn = null) : IDomainEvent;


