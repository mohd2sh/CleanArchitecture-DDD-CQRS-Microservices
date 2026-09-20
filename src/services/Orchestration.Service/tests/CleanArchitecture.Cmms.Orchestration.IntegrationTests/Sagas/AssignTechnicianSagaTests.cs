using CleanArchitecture.Cmms.Contracts.Technicians.Events;
using CleanArchitecture.Cmms.Contracts.WorkOrders.Events;
using CleanArchitecture.Cmms.Orchestration.IntegrationTests.Infrastructure;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace CleanArchitecture.Cmms.Orchestration.IntegrationTests.Sagas;

/// <summary>
/// Integration tests for AssignTechnicianSaga
/// Tests saga state transitions, compensation flows, and timeout handling
/// </summary>
public class AssignTechnicianSagaTests : OrchestrationTestBase
{
    [Fact]
    public async Task AssignTechnicianSaga_ShouldStart_WhenWorkOrderAssignedEventReceived()
    {
        // Arrange
        var harness = ServiceProvider.GetRequiredService<ITestHarness>();
        var workOrderId = Guid.NewGuid();
        var technicianId = Guid.NewGuid();

        var @event = new WorkOrderAssignedEvent(workOrderId, technicianId, DateTime.UtcNow);

        // Act
        await harness.Bus.Publish(@event);
        await harness.Consumed.Any<WorkOrderAssignedEvent>();

        // Assert
        var sagaState = await GetAssignTechnicianSagaStateAsync(workOrderId);
        Assert.NotNull(sagaState);
        Assert.Equal(workOrderId, sagaState.WorkOrderId);
        Assert.Equal(technicianId, sagaState.TechnicianId);
        Assert.Equal("Validating", sagaState.CurrentState);

        // Verify TechnicianAssignedEvent was published
        Assert.True(await harness.Published.Any<TechnicianAssignedEvent>(e => e.Context.Message.WorkOrderId == workOrderId));
    }

    [Fact]
    public async Task AssignTechnicianSaga_ShouldComplete_WhenTechnicianAssignmentValidated()
    {
        // Arrange
        var harness = ServiceProvider.GetRequiredService<ITestHarness>();
        var workOrderId = Guid.NewGuid();
        var technicianId = Guid.NewGuid();

        // Start saga
        await harness.Bus.Publish(new WorkOrderAssignedEvent(workOrderId, technicianId, DateTime.UtcNow));
        await harness.Consumed.Any<WorkOrderAssignedEvent>();

        // Act - Validate assignment
        await harness.Bus.Publish(new TechnicianAssignmentValidatedEvent(workOrderId, technicianId, DateTime.UtcNow));
        await harness.Consumed.Any<TechnicianAssignmentValidatedEvent>();

        // Assert
        var sagaState = await GetAssignTechnicianSagaStateAsync(workOrderId);
        Assert.NotNull(sagaState);
        Assert.Equal("Final", sagaState.CurrentState);
    }

    [Fact]
    public async Task AssignTechnicianSaga_ShouldCompensate_WhenTechnicianAssignmentFailed()
    {
        // Arrange
        var harness = ServiceProvider.GetRequiredService<ITestHarness>();
        var workOrderId = Guid.NewGuid();
        var technicianId = Guid.NewGuid();

        // Start saga
        await harness.Bus.Publish(new WorkOrderAssignedEvent(workOrderId, technicianId, DateTime.UtcNow));
        await harness.Consumed.Any<WorkOrderAssignedEvent>();

        // Act - Fail assignment
        await harness.Bus.Publish(new TechnicianAssignmentFailedEvent(workOrderId, technicianId, "Validation failed", DateTime.UtcNow));
        await harness.Consumed.Any<TechnicianAssignmentFailedEvent>();

        // Assert
        var sagaState = await GetAssignTechnicianSagaStateAsync(workOrderId);
        Assert.NotNull(sagaState);
        Assert.Equal("Final", sagaState.CurrentState);
        Assert.NotNull(sagaState.ErrorMessage);

        // Verify compensation event was published
        Assert.True(await harness.Published.Any<WorkOrderAssignmentFailedEvent>(e => e.Context.Message.WorkOrderId == workOrderId));
    }
}
