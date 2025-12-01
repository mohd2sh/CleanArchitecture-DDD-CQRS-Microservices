using Technicians.Service.Application.Technicians.Dtos;
using CleanArchitecture.Core.Application.Abstractions.Common;

namespace Technicians.Service.Application.Technicians.Queries.GetTechnicianById;

public sealed record GetTechnicianByIdQuery(Guid TechnicianId) : IQuery<Result<TechnicianDto>>;


