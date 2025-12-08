using System.Net;
using CleanArchitecture.Cmms.Api.Technicians.Controllers.V1.Requests.Technicians;
using CleanArchitecture.Cmms.Api.Technicians.IntegrationTests.Infrastructure;
using CleanArchitecture.Cmms.Api.Technicians.IntegrationTests.TestHelpers;
using CleanArchitecture.Cmms.Domain.Technicians.Enums;
using CleanArchitecture.Cmms.Infrastructure.Technicians.Persistence.EfCore;

namespace CleanArchitecture.Cmms.Api.Technicians.IntegrationTests.Api.V1;

/// <summary>
/// Integration tests for Technicians API endpoints
/// Tests HTTP endpoints with real database and full middleware pipeline
/// </summary>
public class TechniciansControllerTests : TechniciansIntegrationTestBase
{
    public TechniciansControllerTests(TechniciansWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task POST_CreateTechnician_Returns200_AndTechnicianId()
    {
        // Arrange
        var request = new CreateTechnicianRequest
        (
         "Test Technician",
          "Senior",
         3
        );

        // Act
        var response = await Client.PostAsJsonAsync(TechniciansApiEndpoints.Technicians.Create(), request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await JsonUtility.DeserializeAsync<ResultDto<Guid>>(response.Content);
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
        Assert.NotEqual(Guid.Empty, result.Value);

        // Verify technician was created in database
        var technician = await WriteDbContext.Technicians.FindAsync(result.Value);
        Assert.NotNull(technician);
        Assert.Equal("Test Technician", technician.Name);
        Assert.Equal(TechnicianStatus.Available, technician.Status);
    }

    [Fact]
    public async Task GET_GetTechnicianById_Returns200_AndTechnicianDetails()
    {
        // Arrange
        var technicianId = await CreateTechnicianAsync("Get By Id Test Technician");

        // Act
        var response = await Client.GetAsync(TechniciansApiEndpoints.Technicians.GetById(technicianId));

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await JsonUtility.DeserializeAsync<ResultDto<object>>(response.Content);
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task GET_GetAvailableTechnicians_Returns200_AndPaginatedList()
    {
        // Arrange
        await CreateTechnicianAsync("Available Tech 1");
        await CreateTechnicianAsync("Available Tech 2");

        // Act
        var response = await Client.GetAsync(TechniciansApiEndpoints.Technicians.GetAvailable(1, 10));

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await JsonUtility.DeserializeAsync<ResultDto<object>>(response.Content);
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task POST_AddCertification_Returns200_AndAddsCertification()
    {
        // Arrange
        var technicianId = await CreateTechnicianAsync("Certification Test Technician");
        var request = new AddCertificationRequest
        (
            "CERT-001",
             DateTime.UtcNow.AddDays(-30),
             DateTime.UtcNow.AddDays(335)
        );

        // Act
        var response = await Client.PostAsJsonAsync(TechniciansApiEndpoints.Technicians.AddCertification(technicianId), request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await JsonUtility.DeserializeAsync<ResultDto>(response.Content);
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);

        // Verify certification was added
        WriteDbContext.ChangeTracker.Clear();
        var technician = await WriteDbContext.Technicians.FindAsync(technicianId);
        Assert.NotNull(technician);
        Assert.Single(technician.Certifications);
        Assert.Equal("CERT-001", technician.Certifications.First().Code);
    }

    [Fact]
    public async Task POST_SetUnavailable_Returns200_AndUpdatesStatus()
    {
        // Arrange
        var technicianId = await CreateTechnicianAsync("Availability Test Technician");

        // Act
        var response = await Client.PostAsync(TechniciansApiEndpoints.Technicians.SetUnavailable(technicianId), null);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await JsonUtility.DeserializeAsync<ResultDto>(response.Content);
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);

        // Verify status was updated
        WriteDbContext.ChangeTracker.Clear();
        var technician = await WriteDbContext.Technicians.FindAsync(technicianId);
        Assert.NotNull(technician);
        Assert.Equal(TechnicianStatus.Unavailable, technician.Status);
    }

    [Fact]
    public async Task POST_SetAvailable_Returns200_AndUpdatesStatus()
    {
        // Arrange
        var technicianId = await CreateTechnicianAsync("Availability Test Technician");

        // Set as unavailable first
        await Client.PostAsync(TechniciansApiEndpoints.Technicians.SetUnavailable(technicianId), null);

        // Act
        var response = await Client.PostAsync(TechniciansApiEndpoints.Technicians.SetAvailable(technicianId), null);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await JsonUtility.DeserializeAsync<ResultDto>(response.Content);
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);

        // Verify status was updated
        WriteDbContext.ChangeTracker.Clear();
        var technician = await WriteDbContext.Technicians.FindAsync(technicianId);
        Assert.NotNull(technician);
        Assert.Equal(TechnicianStatus.Available, technician.Status);
    }
}

