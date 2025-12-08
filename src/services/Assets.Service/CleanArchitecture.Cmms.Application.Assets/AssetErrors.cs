using CleanArchitecture.Core.Application.Abstractions.Common;

namespace CleanArchitecture.Cmms.Application.Assets;

public static class AssetErrors
{
    public static readonly Error NotFound = Error.NotFound("Asset.NotFound", "Asset not found.");
}

