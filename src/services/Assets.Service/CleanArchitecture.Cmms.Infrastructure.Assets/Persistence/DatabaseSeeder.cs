using CleanArchitecture.Cmms.Domain.Assets;
using CleanArchitecture.Cmms.Domain.Assets.ValueObjects;
using CleanArchitecture.Cmms.Infrastructure.Assets.Persistence.EfCore;
using Microsoft.EntityFrameworkCore;

namespace CleanArchitecture.Cmms.Infrastructure.Assets.Persistence;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(WriteDbContext context, CancellationToken ct = default)
    {
        await context.Database.EnsureCreatedAsync(ct);

        if (!await context.Assets.AnyAsync(ct))
        {
            var assets = new List<Asset>
            {
                Asset.Create("Boiler Pump", "Mechanical",
                    AssetTag.Create("ASSET-1001"),
                    AssetLocation.Create("Plant-A", "Floor-1", "Zone-3")),
                Asset.Create("HVAC Unit", "Electrical",
                    AssetTag.Create("ASSET-2002"),
                    AssetLocation.Create("Plant-B", "Roof", "Zone-1"))
            };
            await context.Assets.AddRangeAsync(assets, ct);
            await context.SaveChangesAsync(ct);
        }
    }
}

