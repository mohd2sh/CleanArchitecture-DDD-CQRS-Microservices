namespace WorkOrders.Service.Api.Controllers.V1.Requests.WorkOrders;

public sealed record CreateWorkOrderRequest(Guid AssetId, string Title, string Building, string Floor, string Room);







