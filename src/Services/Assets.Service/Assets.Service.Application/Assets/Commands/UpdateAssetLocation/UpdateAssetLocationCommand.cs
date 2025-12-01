using CleanArchitecture.Core.Application.Abstractions.Common;

namespace Assets.Service.Application.Assets.Commands.UpdateAssetLocation;

public sealed record UpdateAssetLocationCommand(
     Guid AssetId,
     string Site,
     string Area,
     string Zone) : ICommand<Result>;


