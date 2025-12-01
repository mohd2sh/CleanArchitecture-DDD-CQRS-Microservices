using CleanArchitecture.Core.Application.Abstractions.Common;
using CleanArchitecture.Core.Application.Abstractions.Messaging;

namespace WorkOrders.Service.Application.WorkOrders.Commands.CreateWorkOrder;

public sealed record CreateWorkOrderCommand(
    Guid AssetId,
    string Title,
    string Building,
    string Floor,
    string Room
 ) : ICommand<Result<Guid>>;


