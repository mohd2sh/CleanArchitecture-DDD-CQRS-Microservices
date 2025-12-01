using CleanArchitecture.Core.Application.Abstractions.Common;
using CleanArchitecture.Core.Application.Abstractions.Messaging;
using CleanArchitecture.Core.Application.Abstractions.Query;
using WorkOrders.Service.Application.WorkOrders.Dtos;
using WorkOrders.Service.Application.WorkOrders.Interfaces;

namespace WorkOrders.Service.Application.WorkOrders.Queries.GetActiveWorkOrder;

internal sealed class GetActiveWorkOrdersQueryHandler
  : IQueryHandler<GetActiveWorkOrdersQuery, Result<PaginatedList<WorkOrderListItemDto>>>
{
    private readonly IWorkOrderReadRepository _repo;

    public GetActiveWorkOrdersQueryHandler(IWorkOrderReadRepository repo) => _repo = repo;

    public async Task<Result<PaginatedList<WorkOrderListItemDto>>> Handle(GetActiveWorkOrdersQuery request, CancellationToken cancellationToken = default)
    {
        return await _repo.GetActiveWithTechnicianAndAssetAsync(request.Pagination, cancellationToken);
    }
}


