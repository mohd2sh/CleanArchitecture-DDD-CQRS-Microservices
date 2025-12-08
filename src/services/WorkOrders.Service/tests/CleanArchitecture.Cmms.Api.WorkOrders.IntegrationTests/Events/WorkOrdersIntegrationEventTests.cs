using CleanArchitecture.Cmms.Api.WorkOrders.IntegrationTests.Infrastructure;
using CleanArchitecture.Cmms.Application.WorkOrders.Commands.AssignTechnician;
using CleanArchitecture.Cmms.Application.WorkOrders.Commands.CompleteWorkOrder;
using CleanArchitecture.Cmms.Application.WorkOrders.Commands.CreateWorkOrder;
using CleanArchitecture.Cmms.Application.WorkOrders.Commands.StartWorkOrder;
using CleanArchitecture.Cmms.Contracts.WorkOrders.Events;
using Microsoft.EntityFrameworkCore;

namespace CleanArchitecture.Cmms.Api.WorkOrders.IntegrationTests.Events;

public class WorkOrdersIntegrationEventTests : WorkOrdersIntegrationTestBase
{
    public WorkOrdersIntegrationEventTests(WorkOrdersWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task IntegrationEvents_ShouldBeWritten_ToOutboxInTransaction()
    {
        // Arrange
        var assetId = Guid.NewGuid();

        // Act
        var command = new CreateWorkOrderCommand(assetId, "Test Work Order", "B1", "F1", "R1");
        var result = await Mediator.Send(command);

        // Assert
        Assert.True(result.IsSuccess);

        var outboxEvents = await OutboxDbContext.OutboxMessages
            .Where(e => e.EventType.Contains(nameof(WorkOrderCreatedEvent)))
            .ToListAsync();

        Assert.Single(outboxEvents);
        var outboxEvent = outboxEvents[0];

        Assert.Contains(nameof(WorkOrderCreatedEvent), outboxEvent.EventType);
        Assert.NotNull(outboxEvent.Payload);
        Assert.Null(outboxEvent.ProcessedAt);
        Assert.Equal(0, outboxEvent.RetryCount);
    }

    [Fact]
    public async Task MultipleIntegrationEvents_ShouldBeWritten_ToOutbox()
    {
        // Arrange
        var assetId = Guid.NewGuid();
        var technicianId = Guid.NewGuid();

        // Act
        var createCommand = new CreateWorkOrderCommand(assetId, "Test Work Order", "B1", "F1", "R1");
        var workOrderId = await Mediator.Send(createCommand);

        var assignTechnician = new AssignTechnicianCommand(workOrderId.Value, technicianId);
        await Mediator.Send(assignTechnician);

        var startCommand = new StartWorkOrderCommand(workOrderId.Value);
        await Mediator.Send(startCommand);

        var completeCommand = new CompleteWorkOrderCommand(workOrderId.Value);
        await Mediator.Send(completeCommand);

        // Assert
        var outboxEvents = await OutboxDbContext.OutboxMessages
            .OrderBy(e => e.CreatedAt)
            .ToListAsync();

        Assert.True(outboxEvents.Count >= 3);

        var eventTypes = outboxEvents.Select(e => e.EventType).ToList();
        Assert.Contains(eventTypes, t => t.Contains(nameof(WorkOrderCreatedEvent)));
        Assert.Contains(eventTypes, t => t.Contains(nameof(WorkOrderCompletedEvent)));

        Assert.All(outboxEvents, e => Assert.Null(e.ProcessedAt));
        Assert.All(outboxEvents, e => Assert.Equal(0, e.RetryCount));
    }

    [Fact]
    public async Task IntegrationEventPayload_ShouldContain_AllNecessaryData()
    {
        // Arrange
        var assetId = Guid.NewGuid();

        // Act
        var command = new CreateWorkOrderCommand(assetId, "Payload Test", "B1", "F1", "R1");
        var result = await Mediator.Send(command);

        // Assert
        Assert.True(result.IsSuccess);

        var outboxEvent = await OutboxDbContext.OutboxMessages
            .FirstAsync(e => e.EventType.Contains("WorkOrderCreatedEvent"));

        Assert.NotNull(outboxEvent.Payload);

        Assert.Contains("Payload Test", outboxEvent.Payload);
        Assert.Contains(assetId.ToString(), outboxEvent.Payload);
        Assert.Contains(result.Value.ToString(), outboxEvent.Payload);
    }

    [Fact]
    public async Task WhenTransactionRollsBack_OutboxEvents_ShouldNotBeWritten()
    {
        // Arrange
        var assetId = Guid.NewGuid();

        var initialCount = await OutboxDbContext.OutboxMessages.CountAsync();

        // Act

        // Empty title should fail validation
        var command = new CreateWorkOrderCommand(assetId, "", "B1", "F1", "R1");
        var act = () => Mediator.Send(command);

        // Assert
        await Assert.ThrowsAnyAsync<Exception>(act);

        var finalCount = await OutboxDbContext.OutboxMessages.CountAsync();
        Assert.Equal(initialCount, finalCount);
    }
}

