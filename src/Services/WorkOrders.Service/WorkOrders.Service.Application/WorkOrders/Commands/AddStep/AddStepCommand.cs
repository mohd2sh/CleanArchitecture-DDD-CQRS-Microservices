using CleanArchitecture.Core.Application.Abstractions.Common;
using CleanArchitecture.Core.Application.Abstractions.Messaging;

namespace WorkOrders.Service.Application.WorkOrders.Commands.AddStep;

public sealed record AddStepCommand(Guid WorkOrderId, string Title) : ICommand<Result<Guid>>;


