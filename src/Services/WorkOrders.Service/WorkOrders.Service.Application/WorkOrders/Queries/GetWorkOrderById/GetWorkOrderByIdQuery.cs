using CleanArchitecture.Core.Application.Abstractions.Common;
using CleanArchitecture.Core.Application.Abstractions.Messaging;
using WorkOrders.Service.Application.WorkOrders.Dtos;

namespace WorkOrders.Service.Application.WorkOrders.Queries.GetWorkOrderById;

public sealed record GetWorkOrderByIdQuery(Guid Id) : IQuery<Result<WorkOrderDto>>;


