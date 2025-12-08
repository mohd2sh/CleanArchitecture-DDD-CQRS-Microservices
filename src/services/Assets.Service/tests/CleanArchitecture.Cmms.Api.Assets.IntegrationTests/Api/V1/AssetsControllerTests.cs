using System.Net;
using CleanArchitecture.Cmms.Api.Assets.Controllers.V1.Requests.Assets;
using CleanArchitecture.Cmms.Api.Assets.IntegrationTests.Infrastructure;
using CleanArchitecture.Cmms.Api.Assets.IntegrationTests.TestHelpers;
using CleanArchitecture.Cmms.Domain.Assets.Enums;
using CleanArchitecture.Cmms.Infrastructure.Assets.Persistence.EfCore;

namespace CleanArchitecture.Cmms.Api.Assets.IntegrationTests.Api.V1;

/// <summary>
/// Integration tests for Assets API endpoints
/// Tests HTTP endpoints with real database and full middleware pipeline
/// </summary>
public class AssetsControllerTests : AssetsIntegrationTestBase
{
    public AssetsControllerTests(AssetsWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task POST_CreateAsset_Returns200_AndAssetId()
    {
        // Arrange
        var request = new CreateAssetRequest
        (
            "Test Asset",
            "Equipment",
            "TEST-001",
            "Main Site",
            "Production",
            "Zone A"
        );

        // Act
        var response = await Client.PostAsJsonAsync(AssetsApiEndpoints.Assets.Create(), request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await JsonUtility.DeserializeAsync<ResultDto<Guid>>(response.Content);
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
        Assert.NotEqual(Guid.Empty, result.Value);

        // Verify asset was created in database
        var asset = await WriteDbContext.Assets.FindAsync(result.Value);
        Assert.NotNull(asset);
        Assert.Equal("Test Asset", asset.Name);
        Assert.Equal(AssetStatus.Active, asset.Status);
    }

    [Fact]
    public async Task GET_GetAssetById_Returns200_AndAssetDetails()
    {
        // Arrange
        var assetId = await CreateAssetAsync("GET-TEST-001", "Get By Id Test Asset");

        // Act
        var response = await Client.GetAsync(AssetsApiEndpoints.Assets.GetById(assetId));

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await JsonUtility.DeserializeAsync<ResultDto<object>>(response.Content);
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task GET_GetActiveAssets_Returns200_AndPaginatedList()
    {
        // Arrange
        await CreateAssetAsync("ACTIVE-001", "Active Asset 1");
        await CreateAssetAsync("ACTIVE-002", "Active Asset 2");

        // Act
        var response = await Client.GetAsync(AssetsApiEndpoints.Assets.GetActive(1, 10));

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await JsonUtility.DeserializeAsync<ResultDto<object>>(response.Content);
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task PUT_UpdateAssetLocation_Returns200_AndUpdatesLocation()
    {
        // Arrange
        var assetId = await CreateAssetAsync("LOC-TEST-001", "Location Test Asset", site: "Old Site", area: "Old Area", zone: "Old Zone");
        var request = new UpdateAssetLocationRequest
        (
            "New Site",
            "New Area",
            "New Zone"
        );

        // Act
        var response = await Client.PutAsJsonAsync(AssetsApiEndpoints.Assets.UpdateLocation(assetId), request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await JsonUtility.DeserializeAsync<ResultDto>(response.Content);
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);

        // Verify location was updated
        WriteDbContext.ChangeTracker.Clear();
        var asset = await WriteDbContext.Assets.FindAsync(assetId);
        Assert.NotNull(asset);
        Assert.Equal("New Site", asset.Location.Site);
        Assert.Equal("New Area", asset.Location.Area);
        Assert.Equal("New Zone", asset.Location.Zone);
    }
}

