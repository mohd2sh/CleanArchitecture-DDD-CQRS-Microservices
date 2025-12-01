using System.Net;
using Asp.Versioning;
using WorkOrders.Service.Api.Controllers.V1.Requests.WorkOrders;
using WorkOrders.Service.Application.WorkOrders.Commands.AddStep;
using WorkOrders.Service.Application.WorkOrders.Commands.AssignTechnician;
using WorkOrders.Service.Application.WorkOrders.Commands.CompleteStep;
using WorkOrders.Service.Application.WorkOrders.Commands.CompleteWorkOrder;
using WorkOrders.Service.Application.WorkOrders.Commands.CreateWorkOrder;
using WorkOrders.Service.Application.WorkOrders.Commands.StartWorkOrder;
using WorkOrders.Service.Application.WorkOrders.Dtos;
using WorkOrders.Service.Application.WorkOrders.Queries.GetActiveWorkOrder;
using WorkOrders.Service.Application.WorkOrders.Queries.GetWorkOrderById;
using CleanArchitecture.Core.Application.Abstractions.Common;
using CleanArchitecture.Core.Application.Abstractions.Messaging;
using CleanArchitecture.Core.Application.Abstractions.Query;
using Microsoft.AspNetCore.Mvc;

namespace WorkOrders.Service.Api.Controllers.V1;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public sealed class WorkOrdersController : ControllerBase
{
    private readonly IMediator _mediator;

    public WorkOrdersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    [ProducesResponseType(typeof(Result<Guid>), (int)HttpStatusCode.OK)]
    public async Task<IActionResult> Create([FromBody] CreateWorkOrderRequest request, CancellationToken cancellationToken)
    {
        var command = new CreateWorkOrderCommand(request.AssetId, request.Title, request.Building, request.Floor, request.Room);
        var result = await _mediator.Send(command, cancellationToken);
        return Ok(result);
    }

    [HttpPost("{id:guid}/assign")]
    [ProducesResponseType(typeof(Result), (int)HttpStatusCode.OK)]
    public async Task<IActionResult> Assign(Guid id, [FromBody] AssignTechnicianRequest request, CancellationToken cancellationToken)
    {
        var command = new AssignTechnicianCommand(id, request.TechnicianId);
        var result = await _mediator.Send(command, cancellationToken);
        return Ok(result);
    }

    [HttpPost("{id:guid}/start")]
    [ProducesResponseType(typeof(Result), (int)HttpStatusCode.OK)]
    public async Task<IActionResult> Start(Guid id, CancellationToken cancellationToken)
    {
        var command = new StartWorkOrderCommand(id);
        var result = await _mediator.Send(command, cancellationToken);
        return Ok(result);
    }

    [HttpPost("{id:guid}/complete")]
    [ProducesResponseType(typeof(Result), (int)HttpStatusCode.OK)]
    public async Task<IActionResult> Complete(Guid id, CancellationToken cancellationToken)
    {
        var command = new CompleteWorkOrderCommand(id);
        var result = await _mediator.Send(command, cancellationToken);
        return Ok(result);
    }

    [HttpPost("{id:guid}/steps")]
    [ProducesResponseType(typeof(Result<Guid>), (int)HttpStatusCode.OK)]
    public async Task<IActionResult> AddStep(Guid id, [FromBody] AddStepRequest request, CancellationToken cancellationToken)
    {
        var command = new AddStepCommand(id, request.Title);
        var result = await _mediator.Send(command, cancellationToken);
        return Ok(result);
    }

    [HttpPost("{id:guid}/steps/{stepId:guid}/complete")]
    [ProducesResponseType(typeof(Result), (int)HttpStatusCode.OK)]
    public async Task<IActionResult> CompleteStep(Guid id, Guid stepId, CancellationToken cancellationToken)
    {
        var command = new CompleteStepCommand(id, stepId);
        var result = await _mediator.Send(command, cancellationToken);
        return Ok(result);
    }

    [HttpGet]
    [ProducesResponseType(typeof(PaginatedList<WorkOrderListItemDto>), (int)HttpStatusCode.OK)]
    public async Task<IActionResult> GetActive([FromQuery] int pageNumber, [FromQuery] int pageSize, CancellationToken cancellationToken)
    {
        var query = new GetActiveWorkOrdersQuery(new PaginationParam { PageNumber = pageNumber, PageSize = pageSize });
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(Result<WorkOrderDto>), (int)HttpStatusCode.OK)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var query = new GetWorkOrderByIdQuery(id);
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }
}

