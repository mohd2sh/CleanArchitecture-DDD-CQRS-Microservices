using CleanArchitecture.Core.Application.Abstractions.Common;
using CleanArchitecture.Core.Application.Abstractions.Persistence;
using CleanArchitecture.Core.Application.Abstractions.Persistence.Repositories;
using CleanArchitecture.Core.Application.Abstractions.Query;
using Technicians.Service.Application.Technicians.Dtos;
using Technicians.Service.Domain.Technicians;
using Technicians.Service.Domain.Technicians.Enums;

namespace Technicians.Service.Application.Technicians.Queries.GetAvailableTechnicians;

internal sealed class GetAvailableTechniciansQueryHandler
: IQueryHandler<GetAvailableTechniciansQuery, Result<PaginatedList<TechnicianDto>>>
{
    private readonly IReadRepository<Technician, Guid> _repository;

    public GetAvailableTechniciansQueryHandler(IReadRepository<Technician, Guid> repository)
    {
        _repository = repository;
    }

    public async Task<Result<PaginatedList<TechnicianDto>>> Handle(GetAvailableTechniciansQuery request, CancellationToken cancellationToken = default)
    {
        var criteria = Criteria<Technician>.New()
             .Where(p => p.Status == TechnicianStatus.Available)
             .OrderByAsc(p => p.Name)
             .Skip(request.Pagination.Skip)
             .Take(request.Pagination.Take)
             .Build();

        var technicians = await _repository.ListAsync(criteria, cancellationToken);

        var dtoList = technicians.Items.Select(t => new TechnicianDto()
        {
            ActiveAssignmentsCount = t.Assignments.Count(a => !a.IsCompleted),
            Id = t.Id,
            Name = t.Name,
            SkillLevelName = t.SkillLevel.ToString(),
            SkillLevelRank = t.SkillLevel.Rank,
            TotalCertifications = t.Certifications.Count,
            Status = t.Status.ToString()
        }).ToList();

        return technicians.ToNew(dtoList);
    }
}


