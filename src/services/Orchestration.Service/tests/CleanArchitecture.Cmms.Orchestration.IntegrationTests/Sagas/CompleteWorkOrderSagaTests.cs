using CleanArchitecture.Cmms.Contracts.Assets.Events;
using CleanArchitecture.Cmms.Contracts.Technicians.Events;
using CleanArchitecture.Cmms.Contracts.WorkOrders.Events;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;
using Orchestration.Service.Infrastructure;

namespace Orchestration.Service.Sagas;

/// <summary>
/// Integration tests for CompleteWorkOrderSaga
/// Tests complete workflow orchestration, multi-step coordination, and failure scenarios
/// </summary>
public class CompleteWorkOrderSagaTests : OrchestrationTestBase
{
    [Fact]
    public async Task CompleteWorkOrderSaga_ShouldStart_WhenWorkOrderCompletedEventReceived()
    {
        // Arrange
        var harness = ServiceProvider.GetRequiredService<IMassTransitTestHarness>();
        var workOrderId = Guid.NewGuid();
        var assetId = Guid.NewGuid();
        var technicianId = Guid.NewGuid();

        var @event = new WorkOrderCompletedEvent(workOrderId, assetId, technicianId, DateTime.UtcNow);

        // Act
        await harness.Bus.Publish(@event);
        await harness.Consumed.Any<WorkOrderCompletedEvent>();

        // Assert
        var sagaState = await GetSagaStateAsync(workOrderId);
        Assert.NotNull(sagaState);
        Assert.Equal(workOrderId, sagaState.WorkOrderId);
        Assert.Equal(assetId, sagaState.AssetId);
        Assert.Equal(technicianId, sagaState.TechnicianId);
        Assert.Equal("Processing", sagaState.CurrentState);
        Assert.False(sagaState.AssetCompleted);
        Assert.False(sagaState.TechnicianCompleted);

        // Verify WorkOrderCompletionRequestedEvent was published
        Assert.True(await harness.Published.Any<WorkOrderCompletionRequestedEvent>(e => e.Context.Message.WorkOrderId == workOrderId));
    }

    [Fact]
    public async Task CompleteWorkOrderSaga_ShouldComplete_WhenBothServicesComplete()
    {
        // Arrange
        var harness = ServiceProvider.GetRequiredService<IMassTransitTestHarness>();
        var workOrderId = Guid.NewGuid();
        var assetId = Guid.NewGuid();
        var technicianId = Guid.NewGuid();

        // Start saga
        await harness.Bus.Publish(new WorkOrderCompletedEvent(workOrderId, assetId, technicianId, DateTime.UtcNow));
        await harness.Consumed.Any<WorkOrderCompletedEvent>();

        // Act - Both services complete
        await harness.Bus.Publish(new AssetMaintenanceCompletedEvent(workOrderId, assetId, DateTime.UtcNow));
        await harness.Consumed.Any<AssetMaintenanceCompletedEvent>();

        await harness.Bus.Publish(new TechnicianAssignmentCompletedEvent(workOrderId, technicianId, DateTime.UtcNow));
        await harness.Consumed.Any<TechnicianAssignmentCompletedEvent>();

        // Assert
        var sagaState = await GetSagaStateAsync(workOrderId);
        Assert.NotNull(sagaState);
        Assert.Equal("Final", sagaState.CurrentState);
        Assert.True(sagaState.AssetCompleted);
        Assert.True(sagaState.TechnicianCompleted);
    }

    [Fact]
    public async Task CompleteWorkOrderSaga_ShouldCompensate_WhenAssetMaintenanceFails()
    {
        // Arrange
        var harness = ServiceProvider.GetRequiredService<IMassTransitTestHarness>();
        var workOrderId = Guid.NewGuid();
        var assetId = Guid.NewGuid();
        var technicianId = Guid.NewGuid();

        // Start saga
        await harness.Bus.Publish(new WorkOrderCompletedEvent(workOrderId, assetId, technicianId, DateTime.UtcNow));
        await harness.Consumed.Any<WorkOrderCompletedEvent>();

        // Act - Asset maintenance fails
        await harness.Bus.Publish(new AssetMaintenanceFailedEvent(workOrderId, assetId, "Maintenance failed", DateTime.UtcNow));
        await harness.Consumed.Any<AssetMaintenanceFailedEvent>();

        // Assert
        var sagaState = await GetSagaStateAsync(workOrderId);
        Assert.NotNull(sagaState);
        Assert.Equal("Final", sagaState.CurrentState);
        Assert.NotNull(sagaState.ErrorMessage);

        // Verify compensation event was published
        Assert.True(await harness.Published.Any<WorkOrderCompletingFailedEvent>(e => e.Context.Message.WorkOrderId == workOrderId));
    }

    [Fact]
    public async Task CompleteWorkOrderSaga_ShouldCompensate_WhenTechnicianAssignmentFails()
    {
        // Arrange
        var harness = ServiceProvider.GetRequiredService<IMassTransitTestHarness>();
        var workOrderId = Guid.NewGuid();
        var assetId = Guid.NewGuid();
        var technicianId = Guid.NewGuid();

        // Start saga
        await harness.Bus.Publish(new WorkOrderCompletedEvent(workOrderId, assetId, technicianId, DateTime.UtcNow));
        await harness.Consumed.Any<WorkOrderCompletedEvent>();

        // Act - Technician assignment fails
        await harness.Bus.Publish(new TechnicianAssignmentFailedEvent(workOrderId, technicianId, "Assignment failed", DateTime.UtcNow));
        await harness.Consumed.Any<TechnicianAssignmentFailedEvent>();

        // Assert
        var sagaState = await GetSagaStateAsync(workOrderId);
        Assert.NotNull(sagaState);
        Assert.Equal("Final", sagaState.CurrentState);
        Assert.NotNull(sagaState.ErrorMessage);

        // Verify compensation event was published
        Assert.True(await harness.Published.Any<WorkOrderCompletingFailedEvent>(e => e.Context.Message.WorkOrderId == workOrderId));
    }
}

