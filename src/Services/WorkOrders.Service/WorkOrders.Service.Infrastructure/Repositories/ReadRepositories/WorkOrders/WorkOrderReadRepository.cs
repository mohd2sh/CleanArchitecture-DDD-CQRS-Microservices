using System.Data;
using CleanArchitecture.Core.Application.Abstractions.Query;
using WorkOrders.Service.Application.WorkOrders.Dtos;
using WorkOrders.Service.Application.WorkOrders.Interfaces;
using WorkOrders.Service.Domain.WorkOrders.Enums;

namespace WorkOrders.Service.Infrastructure.Repositories.ReadRepositories.WorkOrders;

internal sealed class WorkOrderReadRepository : DapperBaseRepository, IWorkOrderReadRepository
{
    public WorkOrderReadRepository(IDbConnection connection) : base(connection) { }

    public async Task<WorkOrderDto?> GetWorkOrderById(Guid id, CancellationToken ct)
    {
        const string sql = "SELECT Id, Title, Status FROM workorders.WorkOrders WHERE Id = @Id";

        return await QuerySingleAsync<WorkOrderDto>(sql, param: new { Id = id }, ct: ct);
    }

    public async Task<PaginatedList<WorkOrderListItemDto>> GetActiveWithTechnicianAndAssetAsync(PaginationParam pagination, CancellationToken ct)
    {
        //TODO:: Move to CDC 
        // Note: In microservices, we can't join with Technicians and Assets tables (they're in different services)
        // For now, return WorkOrders data only. Later, we can use denormalized read models or event-driven updates
        const string sql = @"
            SELECT 
                w.Id,
                w.Title,
                '' AS TechnicianName,  -- Will be populated via denormalized read model or event updates
                '' AS AssetName,       -- Will be populated via denormalized read model or event updates
                w.Status
            FROM workorders.WorkOrders w
            WHERE w.Status <> @Completed";

        var param = new { Completed = WorkOrderStatus.Completed.ToString() };

        return await QueryPaginatedAsync<WorkOrderListItemDto>(
            baseSql: sql,
            param: param,
            pageNumber: pagination.PageNumber,
            pageSize: pagination.PageSize,
            orderBy: "w.Id DESC",
            ct: ct);
    }
}







