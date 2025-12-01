namespace Assets.Service.Api.Controllers.V1.Requests.Assets;

public sealed record UpdateAssetLocationRequest(
    string Site,
    string Area,
    string Zone);


