using Technicians.Service.Application.Technicians.Dtos;
using Technicians.Service.Domain.Technicians;
using CleanArchitecture.Core.Application.Abstractions.Common;
using CleanArchitecture.Core.Application.Abstractions.Persistence;
using CleanArchitecture.Core.Application.Abstractions.Persistence.Repositories;
using CleanArchitecture.Core.Application.Abstractions.Query;

namespace Technicians.Service.Application.Technicians.Queries.GetTechnicianAssignments;

internal sealed class GetTechnicianAssignmentsQueryHandler
 : IQueryHandler<GetTechnicianAssignmentsQuery, Result<PaginatedList<TechnicianAssignmentDto>>>
{
    private readonly IReadRepository<Technician, Guid> _repository;

    public GetTechnicianAssignmentsQueryHandler(IReadRepository<Technician, Guid> repository)
    {
        _repository = repository;
    }

    public async Task<Result<PaginatedList<TechnicianAssignmentDto>>> Handle(
        GetTechnicianAssignmentsQuery request,
        CancellationToken cancellationToken = default)
    {
        var criteria = Criteria<Technician>.New()
            .Where(p => p.Id == request.TechnicianId)
            .Build();

        var technician = await _repository.FirstOrDefaultAsync(criteria, cancellationToken);
        if (technician == null)
            return TechnicianErrors.NotFound;

        var assignments = technician.Assignments
            .Where(a => !request.OnlyActive || !a.IsCompleted)
            .OrderByDescending(a => a.AssignedOn)
            .Skip(request.Pagination.Skip)
            .Take(request.Pagination.Take)
            .Select(a => new TechnicianAssignmentDto
            {
                WorkOrderId = a.WorkOrderId,
                AssignedOn = a.AssignedOn,
                CompletedOn = a.CompletedOn
            })
            .ToList();

        var totalCount = request.OnlyActive
            ? technician.Assignments.Count(a => !a.IsCompleted)
            : technician.Assignments.Count;

        var paginatedList = PaginatedList<TechnicianAssignmentDto>.Create(
            assignments,
            totalCount,
            request.Pagination.PageNumber,
            request.Pagination.PageSize);

        return paginatedList;
    }
}


