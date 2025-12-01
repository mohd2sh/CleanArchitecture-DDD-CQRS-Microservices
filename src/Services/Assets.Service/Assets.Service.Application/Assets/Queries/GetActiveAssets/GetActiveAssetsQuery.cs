using Assets.Service.Application.Assets.Dtos;
using CleanArchitecture.Core.Application.Abstractions.Common;
using CleanArchitecture.Core.Application.Abstractions.Query;

namespace Assets.Service.Application.Assets.Queries.GetActiveAssets;

public sealed record GetActiveAssetsQuery(PaginationParam Pagination)
: IQuery<Result<PaginatedList<AssetDto>>>;


