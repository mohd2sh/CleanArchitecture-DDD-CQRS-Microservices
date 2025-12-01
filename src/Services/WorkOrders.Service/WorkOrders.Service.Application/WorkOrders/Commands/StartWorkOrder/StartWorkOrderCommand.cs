using CleanArchitecture.Core.Application.Abstractions.Common;
using CleanArchitecture.Core.Application.Abstractions.Messaging;

namespace WorkOrders.Service.Application.WorkOrders.Commands.StartWorkOrder;

public sealed record StartWorkOrderCommand(Guid WorkOrderId) : ICommand<Result>;


