using CleanArchitecture.Core.Application.Abstractions.Common;
using CleanArchitecture.Core.Application.Abstractions.Messaging;
using WorkOrders.Service.Application.WorkOrders.Dtos;
using WorkOrders.Service.Application.WorkOrders.Interfaces;

namespace WorkOrders.Service.Application.WorkOrders.Queries.GetWorkOrderById;

internal sealed class GetWorkOrderByIdQueryHandler
 : IQueryHandler<GetWorkOrderByIdQuery, Result<WorkOrderDto>>
{
    private readonly IWorkOrderReadRepository _repository;

    public GetWorkOrderByIdQueryHandler(IWorkOrderReadRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<WorkOrderDto>> Handle(GetWorkOrderByIdQuery request, CancellationToken cancellationToken = default)
    {
        var entity = await _repository.GetWorkOrderById(request.Id, cancellationToken);

        if (entity is null)
            return WorkOrderErrors.NotFound;

        var dto = new WorkOrderDto(entity.Id, entity.Title, entity.Status);

        return dto;
    }
}


