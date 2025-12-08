using CleanArchitecture.Cmms.Api.WorkOrders.IntegrationTests.Infrastructure;
using CleanArchitecture.Cmms.Application.WorkOrders.Commands.AddStep;
using CleanArchitecture.Cmms.Application.WorkOrders.Commands.AssignTechnician;
using CleanArchitecture.Cmms.Application.WorkOrders.Commands.CompleteStep;
using CleanArchitecture.Cmms.Application.WorkOrders.Commands.CompleteWorkOrder;
using CleanArchitecture.Cmms.Application.WorkOrders.Commands.CreateWorkOrder;
using CleanArchitecture.Cmms.Application.WorkOrders.Commands.StartWorkOrder;
using CleanArchitecture.Cmms.Contracts.WorkOrders.Events;
using CleanArchitecture.Cmms.Domain.WorkOrders.Enums;
using CleanArchitecture.Cmms.Infrastructure.WorkOrders.Persistence.EfCore;
using Microsoft.EntityFrameworkCore;

namespace CleanArchitecture.Cmms.Api.WorkOrders.IntegrationTests.Transactions;

public class WorkOrdersTransactionTests : WorkOrdersIntegrationTestBase
{
    public WorkOrdersTransactionTests(WorkOrdersWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task DomainEvents_AndDatabaseChanges_ShouldBeAtomic()
    {
        // Arrange
        var assetId = Guid.NewGuid();

        // Act
        var command = new CreateWorkOrderCommand(assetId, "Atomic Test", "B1", "F1", "R1");
        var result = await Mediator.Send(command);

        // Assert
        Assert.True(result.IsSuccess);

        var workOrder = await WriteDbContext.WorkOrders.FindAsync(result.Value);
        Assert.NotNull(workOrder);
        Assert.Equal("Atomic Test", workOrder.Title);
        Assert.Equal(WorkOrderStatus.Open, workOrder.Status);

        var outboxEvent = await OutboxDbContext.OutboxMessages
            .FirstOrDefaultAsync(e => e.EventType.Contains(nameof(WorkOrderCreatedEvent)));
        Assert.NotNull(outboxEvent);
        Assert.Contains("Atomic Test", outboxEvent.Payload);
    }

    [Fact]
    public async Task MultipleDomainEvents_ShouldBeAtomic_WithDatabaseChanges()
    {
        // Arrange
        var assetId = Guid.NewGuid();
        var technicianId = Guid.NewGuid();

        // Act 
        var createCommand = new CreateWorkOrderCommand(assetId, "Multi Event Test", "B1", "F1", "R1");
        var workOrderId = await Mediator.Send(createCommand);

        var assignTechnicianCommand = new AssignTechnicianCommand(workOrderId.Value, technicianId);
        await Mediator.Send(assignTechnicianCommand);

        var startCommand = new StartWorkOrderCommand(workOrderId.Value);
        await Mediator.Send(startCommand);

        var completeCommand = new CompleteWorkOrderCommand(workOrderId.Value);
        await Mediator.Send(completeCommand);

        //Assert
        var workOrder = await WriteDbContext.WorkOrders.FindAsync(workOrderId.Value);
        Assert.Equal(WorkOrderStatus.Completed, workOrder.Status);

        var outboxEvents = await OutboxDbContext.OutboxMessages
            .OrderBy(e => e.CreatedAt)
            .ToListAsync();

        Assert.True(outboxEvents.Count >= 3);
        Assert.Contains(outboxEvents, e => e.EventType.Contains(nameof(WorkOrderCreatedEvent)));
        Assert.Contains(outboxEvents, e => e.EventType.Contains(nameof(WorkOrderCompletedEvent)));
    }

    [Fact]
    public async Task WorkOrderSteps_ShouldBeAtomic_WithWorkOrderChanges()
    {
        // Arrange
        var assetId = Guid.NewGuid();
        var createCommand = new CreateWorkOrderCommand(assetId, "Steps Test", "B1", "F1", "R1");
        var workOrderId = await Mediator.Send(createCommand);

        // Act
        var addStepCommand = new AddStepCommand(workOrderId.Value, "Test Step");
        var stepId = await Mediator.Send(addStepCommand);

        var completeStepCommand = new CompleteStepCommand(workOrderId.Value, stepId.Value);
        await Mediator.Send(completeStepCommand);

        // Assert
        WriteDbContext.ChangeTracker.Clear();
        var workOrder = await WriteDbContext.WorkOrders.FindAsync(workOrderId.Value);
        Assert.NotNull(workOrder);
        Assert.Single(workOrder.Steps);
        Assert.True(workOrder.Steps.First().Completed);
    }
}

