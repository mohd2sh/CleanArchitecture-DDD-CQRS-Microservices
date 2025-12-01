using CleanArchitecture.Core.Application.Abstractions.Common;
using CleanArchitecture.Core.Application.Abstractions.Messaging;
using CleanArchitecture.Core.Application.Abstractions.Persistence.Repositories;
using WorkOrders.Service.Domain.WorkOrders;
using WorkOrders.Service.Domain.WorkOrders.ValueObjects;

namespace WorkOrders.Service.Application.WorkOrders.Commands.CreateWorkOrder;

internal sealed class CreateWorkOrderCommandHandler : ICommandHandler<CreateWorkOrderCommand, Result<Guid>>
{
    private readonly IRepository<WorkOrder, Guid> _workOrderRepository;

    public CreateWorkOrderCommandHandler(IRepository<WorkOrder, Guid> workOrderRepository)
    {
        _workOrderRepository = workOrderRepository;
    }

    public async Task<Result<Guid>> Handle(CreateWorkOrderCommand request, CancellationToken cancellationToken = default)
    {
        var location = Location.Create(request.Building, request.Floor, request.Room);

        var workOrder = WorkOrder.Create(request.AssetId, request.Title, location);

        await _workOrderRepository.AddAsync(workOrder, cancellationToken);

        return workOrder.Id;
    }
}


