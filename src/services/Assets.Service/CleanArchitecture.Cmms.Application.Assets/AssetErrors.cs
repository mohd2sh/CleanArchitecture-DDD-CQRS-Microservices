using CleanArchitecture.Core.Application.Abstractions.Common;
using CleanArchitecture.Core.Domain.Abstractions.Attributes;

namespace CleanArchitecture.Cmms.Application.Assets;

[ErrorCodeDefinition("Asset")]
public static class AssetErrors
{
    [ApplicationError]
    public static readonly Error NotFound = Error.NotFound("Asset.NotFound", "Asset not found.");
}

