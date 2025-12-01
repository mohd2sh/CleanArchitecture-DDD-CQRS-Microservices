using WorkOrders.Service.Application.WorkOrders.Dtos;
using CleanArchitecture.Core.Application.Abstractions.Persistence.Repositories;
using CleanArchitecture.Core.Application.Abstractions.Query;

namespace WorkOrders.Service.Application.WorkOrders.Interfaces;

public interface IWorkOrderReadRepository : IReadRepository
{
    Task<PaginatedList<WorkOrderListItemDto>> GetActiveWithTechnicianAndAssetAsync(PaginationParam pagination, CancellationToken ct);
    Task<WorkOrderDto?> GetWorkOrderById(Guid id, CancellationToken ct);
}







