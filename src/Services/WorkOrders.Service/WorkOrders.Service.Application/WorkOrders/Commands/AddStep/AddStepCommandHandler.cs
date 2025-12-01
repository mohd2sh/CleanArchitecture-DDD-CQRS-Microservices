using CleanArchitecture.Core.Application.Abstractions.Common;
using CleanArchitecture.Core.Application.Abstractions.Messaging;
using CleanArchitecture.Core.Application.Abstractions.Persistence.Repositories;
using WorkOrders.Service.Domain.WorkOrders;

namespace WorkOrders.Service.Application.WorkOrders.Commands.AddStep;

internal sealed class AddStepCommandHandler
 : ICommandHandler<AddStepCommand, Result<Guid>>
{
    private readonly IRepository<WorkOrder, Guid> _repository;

    public AddStepCommandHandler(IRepository<WorkOrder, Guid> repository)
    {
        _repository = repository;
    }

    public async Task<Result<Guid>> Handle(AddStepCommand request, CancellationToken cancellationToken = default)
    {
        var workOrder = await _repository.GetByIdAsync(request.WorkOrderId, cancellationToken);

        if (workOrder is null)
            return WorkOrderErrors.NotFound;

        var stepId = workOrder.AddStep(request.Title);

        await _repository.UpdateAsync(workOrder, cancellationToken);

        return stepId;
    }
}


