using CleanArchitecture.Core.Application.Abstractions.Common;
using CleanArchitecture.Core.Application.Abstractions.Messaging;

namespace WorkOrders.Service.Application.WorkOrders.Commands.AssignTechnician;

public sealed record AssignTechnicianCommand(Guid WorkOrderId, Guid TechnicianId)
: ICommand<Result>;


