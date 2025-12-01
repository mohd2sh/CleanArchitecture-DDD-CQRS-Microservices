using CleanArchitecture.Core.Application.Abstractions.Common;
using CleanArchitecture.Core.Application.Abstractions.Messaging;
using CleanArchitecture.Core.Application.Abstractions.Persistence.Repositories;
using WorkOrders.Service.Domain.WorkOrders;

namespace WorkOrders.Service.Application.WorkOrders.Commands.StartWorkOrder;

internal sealed class StartWorkOrderCommandHandler : ICommandHandler<StartWorkOrderCommand, Result>
{
    private readonly IRepository<WorkOrder, Guid> _repository;

    public StartWorkOrderCommandHandler(IRepository<WorkOrder, Guid> repository)
    {
        _repository = repository;
    }

    public async Task<Result> Handle(StartWorkOrderCommand request, CancellationToken cancellationToken = default)
    {
        var workOrder = await _repository.GetByIdAsync(request.WorkOrderId, cancellationToken);

        if (workOrder is null)
            return WorkOrderErrors.NotFound;

        workOrder.Start();

        await _repository.UpdateAsync(workOrder, cancellationToken);

        return Result.Success();
    }
}


