using Microsoft.EntityFrameworkCore;

namespace WorkOrders.Service.Infrastructure.Persistence.EfCore;

public sealed class ReadDbContext : WorkOrdersDbContextBase
{
    public ReadDbContext(DbContextOptions<ReadDbContext> options) : base(options) { }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);

        base.OnConfiguring(optionsBuilder);
    }
}







