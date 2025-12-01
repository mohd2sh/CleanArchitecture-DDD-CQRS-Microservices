using Technicians.Service.Application.Technicians.Dtos;
using CleanArchitecture.Core.Application.Abstractions.Common;
using CleanArchitecture.Core.Application.Abstractions.Query;

namespace Technicians.Service.Application.Technicians.Queries.GetAvailableTechnicians;

public sealed record GetAvailableTechniciansQuery(PaginationParam Pagination) : IQuery<Result<PaginatedList<TechnicianDto>>>;


