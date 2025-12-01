using CleanArchitecture.Core.Application.Abstractions.Common;
using CleanArchitecture.Core.Application.Abstractions.Messaging;

namespace WorkOrders.Service.Application.WorkOrders.Commands.CompleteStep;

public sealed record CompleteStepCommand(Guid WorkOrderId, Guid StepId) : ICommand<Result>;


