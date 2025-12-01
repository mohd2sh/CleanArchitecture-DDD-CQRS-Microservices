using CleanArchitecture.Core.Application.Abstractions.Common;
using CleanArchitecture.Core.Application.Abstractions.Messaging;
using CleanArchitecture.Core.Application.Abstractions.Query;
using WorkOrders.Service.Application.WorkOrders.Dtos;

namespace WorkOrders.Service.Application.WorkOrders.Queries.GetActiveWorkOrder;

public sealed record GetActiveWorkOrdersQuery(PaginationParam Pagination)
: IQuery<Result<PaginatedList<WorkOrderListItemDto>>>;


