namespace Assets.Service.Api.Controllers.V1.Requests.Assets;

public sealed record CreateAssetRequest(
    string Name,
    string Type,
    string TagCode,
    string Site,
    string Area,
    string Zone);


