using System.Net;
using CleanArchitecture.Cmms.Api.WorkOrders.Controllers.V1.Requests.WorkOrders;
using CleanArchitecture.Cmms.Api.WorkOrders.IntegrationTests.Infrastructure;
using CleanArchitecture.Cmms.Api.WorkOrders.IntegrationTests.TestHelpers;
using CleanArchitecture.Cmms.Domain.WorkOrders.Enums;
using CleanArchitecture.Cmms.Infrastructure.WorkOrders.Persistence.EfCore;

namespace CleanArchitecture.Cmms.Api.WorkOrders.IntegrationTests.Api.V1;

public class WorkOrdersControllerTests : WorkOrdersIntegrationTestBase
{
    public WorkOrdersControllerTests(WorkOrdersWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task POST_CreateWorkOrder_Returns200_AndWorkOrderId()
    {
        // Arrange
        var assetId = Guid.NewGuid(); // In real scenario, this would come from Assets service
        var request = new CreateWorkOrderRequest
        (
            assetId,
            "Test Work Order",
            "Building A",
            "Floor 1",
            "Room 101"
        );

        // Act
        var response = await Client.PostAsJsonAsync(WorkOrdersApiEndpoints.WorkOrders.Create(), request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await JsonUtility.DeserializeAsync<ResultDto<Guid>>(response.Content);
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
        Assert.NotEqual(Guid.Empty, result.Value);

        var workOrder = await WriteDbContext.WorkOrders.FindAsync(result.Value);
        Assert.NotNull(workOrder);
        Assert.Equal("Test Work Order", workOrder.Title);
        Assert.Equal(WorkOrderStatus.Open, workOrder.Status);
        Assert.Equal(assetId, workOrder.AssetId);
    }

    [Fact]
    public async Task GET_GetWorkOrderById_Returns200_AndWorkOrderDetails()
    {
        // Arrange
        var assetId = Guid.NewGuid();
        var workOrderId = await CreateWorkOrderAsync(assetId, "Get By Id Test", "B1", "F1", "R1");

        // Act
        var response = await Client.GetAsync(WorkOrdersApiEndpoints.WorkOrders.GetById(workOrderId));

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await JsonUtility.DeserializeAsync<ResultDto<object>>(response.Content);
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task GET_GetActiveWorkOrders_Returns200_AndPaginatedList()
    {
        // Arrange
        var assetId1 = Guid.NewGuid();
        var assetId2 = Guid.NewGuid();
        await CreateWorkOrderAsync(assetId1, "Active Work Order 1", "B1", "F1", "R1");
        await CreateWorkOrderAsync(assetId2, "Active Work Order 2", "B1", "F1", "R2");

        // Act
        var response = await Client.GetAsync(WorkOrdersApiEndpoints.WorkOrders.GetActive(1, 10));

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await JsonUtility.DeserializeAsync<ResultDto<object>>(response.Content);
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task POST_AddStep_Returns200_AndStepId()
    {
        // Arrange
        var assetId = Guid.NewGuid();
        var workOrderId = await CreateWorkOrderAsync(assetId, "Add Step Test", "B1", "F1", "R1");
        var request = new AddStepRequest("Test Step");

        // Act
        var response = await Client.PostAsJsonAsync(WorkOrdersApiEndpoints.WorkOrders.AddStep(workOrderId), request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await JsonUtility.DeserializeAsync<ResultDto<Guid>>(response.Content);
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
        Assert.NotEqual(Guid.Empty, result.Value);

        WriteDbContext.ChangeTracker.Clear();
        var workOrder = await WriteDbContext.WorkOrders.FindAsync(workOrderId);
        Assert.NotNull(workOrder);
        Assert.Single(workOrder.Steps);
        Assert.Equal("Test Step", workOrder.Steps.First().Description);
    }

    [Fact]
    public async Task POST_StartWorkOrder_Returns400_WhenNotAssigned()
    {
        // Arrange
        var assetId = Guid.NewGuid();
        var workOrderId = await CreateWorkOrderAsync(assetId, "Start Test", "B1", "F1", "R1");

        // Act
        var response = await Client.PostAsync(WorkOrdersApiEndpoints.WorkOrders.Start(workOrderId), null);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task POST_StartWorkOrder_Returns200_AndUpdatesStatus()
    {
        // Arrange
        var assetId = Guid.NewGuid();
        var technicianId = Guid.NewGuid();
        var workOrderId = await CreateWorkOrderAsync(assetId, "Start Test", "B1", "F1", "R1");
        await AssignWorkOrderAsync(workOrderId, technicianId);

        // Act
        var response = await Client.PostAsync(WorkOrdersApiEndpoints.WorkOrders.Start(workOrderId), null);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await JsonUtility.DeserializeAsync<ResultDto>(response.Content);
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);

        WriteDbContext.ChangeTracker.Clear();
        var workOrder = await WriteDbContext.WorkOrders.FindAsync(workOrderId);
        Assert.NotNull(workOrder);
        Assert.Equal(WorkOrderStatus.InProgress, workOrder.Status);
    }

    [Fact]
    public async Task POST_CompleteWorkOrder_Returns200_AndUpdatesStatus()
    {
        // Arrange
        var assetId = Guid.NewGuid();
        var technicianId = Guid.NewGuid();
        var workOrderId = await CreateWorkOrderAsync(assetId, "Complete Test", "B1", "F1", "R1");
        await AssignWorkOrderAsync(workOrderId, technicianId);
        await StartWorkOrderAsync(workOrderId);

        // Start the work order first
        await Client.PostAsync(WorkOrdersApiEndpoints.WorkOrders.Start(workOrderId), null);

        // Act
        var response = await Client.PostAsync(WorkOrdersApiEndpoints.WorkOrders.Complete(workOrderId), null);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await JsonUtility.DeserializeAsync<ResultDto>(response.Content);
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);

        WriteDbContext.ChangeTracker.Clear();
        var workOrder = await WriteDbContext.WorkOrders.FindAsync(workOrderId);
        Assert.NotNull(workOrder);
        Assert.Equal(WorkOrderStatus.Completed, workOrder.Status);
    }
}

