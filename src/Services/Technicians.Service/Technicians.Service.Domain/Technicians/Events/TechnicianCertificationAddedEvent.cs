using CleanArchitecture.Core.Domain.Abstractions;

namespace Technicians.Service.Domain.Technicians.Events;

public sealed record TechnicianCertificationAddedEvent(Guid TechnicianId, string CertificationCode, DateTime? OccurredOn = null) : IDomainEvent;


