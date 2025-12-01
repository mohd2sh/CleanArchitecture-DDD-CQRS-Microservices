using Assets.Service.Application.Assets.Dtos;
using CleanArchitecture.Core.Application.Abstractions.Common;

namespace Assets.Service.Application.Assets.Queries.GetAssetById;

public sealed record GetAssetByIdQuery(Guid AssetId) : IQuery<Result<AssetDto>>;


