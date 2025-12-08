using CleanArchitecture.Cmms.Application.WorkOrders.Dtos;
using CleanArchitecture.Cmms.Application.WorkOrders.Interfaces;
using CleanArchitecture.Cmms.Application.WorkOrders.Queries.GetWorkOrderById;

namespace CleanArchitecture.Cmms.Application.WorkOrders.UnitTests.Queries.GetWorkOrderById;

public class GetWorkOrderByIdQueryHandlerTests
{
    private readonly Mock<IWorkOrderReadRepository> _repositoryMock;
    private readonly GetWorkOrderByIdQueryHandler _sut;

    public GetWorkOrderByIdQueryHandlerTests()
    {
        _repositoryMock = new Mock<IWorkOrderReadRepository>();
        _sut = new GetWorkOrderByIdQueryHandler(_repositoryMock.Object);
    }

    [Fact]
    public async Task Handle_WhenWorkOrderExists_ShouldReturnWorkOrderDto()
    {
        // Arrange
        var workOrderId = Guid.NewGuid();
        var query = new GetWorkOrderByIdQuery(workOrderId);

        var workOrderEntity = new WorkOrderDto(workOrderId, "Test Work Order", "InProgress");

        _repositoryMock.Setup(x => x.GetWorkOrderById(workOrderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(workOrderEntity);

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Id.Should().Be(workOrderId);
        result.Value.Title.Should().Be("Test Work Order");
        result.Value.Status.Should().Be("InProgress");

        _repositoryMock.Verify(x => x.GetWorkOrderById(workOrderId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenWorkOrderNotFound_ShouldReturnFailure()
    {
        // Arrange
        var workOrderId = Guid.NewGuid();
        var query = new GetWorkOrderByIdQuery(workOrderId);

        _repositoryMock.Setup(x => x.GetWorkOrderById(workOrderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((WorkOrderDto?)null);

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNull();
        result.Error!.Code.Should().Be(WorkOrderErrors.NotFound.Code);
        result.Error.Message.Should().Be(WorkOrderErrors.NotFound.Message);

        _repositoryMock.Verify(x => x.GetWorkOrderById(workOrderId, It.IsAny<CancellationToken>()), Times.Once);
    }
}
