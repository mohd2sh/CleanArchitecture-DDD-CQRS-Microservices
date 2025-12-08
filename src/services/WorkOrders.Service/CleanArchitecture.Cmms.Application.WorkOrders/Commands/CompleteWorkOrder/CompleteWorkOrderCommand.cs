using CleanArchitecture.Core.Application.Abstractions.Common;
using CleanArchitecture.Core.Application.Abstractions.Messaging;

namespace CleanArchitecture.Cmms.Application.WorkOrders.Commands.CompleteWorkOrder;

public sealed record CompleteWorkOrderCommand(Guid WorkOrderId) : ICommand<Result>;

