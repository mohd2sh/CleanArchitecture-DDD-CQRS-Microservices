using CleanArchitecture.Core.Application.Abstractions.Common;

namespace Assets.Service.Application.Assets;

public static class AssetErrors
{
    public static readonly Error NotFound = Error.NotFound("Asset.NotFound", "Asset not found.");
}


