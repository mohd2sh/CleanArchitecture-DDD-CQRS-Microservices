using CleanArchitecture.Cmms.Domain.WorkOrders;
using CleanArchitecture.Cmms.Domain.WorkOrders.ValueObjects;
using CleanArchitecture.Cmms.Infrastructure.WorkOrders.Persistence.EfCore;
using Microsoft.EntityFrameworkCore;

namespace CleanArchitecture.Cmms.Infrastructure.WorkOrders.Persistence;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(WriteDbContext context, CancellationToken ct = default)
    {
        await context.Database.EnsureCreatedAsync(ct);
        //TODO move this to db
        if (!await context.WorkOrders.AnyAsync(ct))
        {
            var assetId1 = Guid.Parse("2915bd5e-b291-47ec-9e2c-0e08f2cfa2ee");
            var assetId2 = Guid.NewGuid(); // Placeholder - would come from Assets service

            var workOrders = new List<WorkOrder>
            {
                WorkOrder.Create(
                    assetId1,
                    "Replace filter on HVAC Unit",
                    Location.Create("Plant-A", "Floor-1", "Zone-3")),
                WorkOrder.Create(
                    assetId2,
                    "Inspect boiler pump",
                    Location.Create("Plant-B", "Roof", "Zone-1"))
            };

            await context.WorkOrders.AddRangeAsync(workOrders, ct);
            await context.SaveChangesAsync(ct);
        }
    }
}

