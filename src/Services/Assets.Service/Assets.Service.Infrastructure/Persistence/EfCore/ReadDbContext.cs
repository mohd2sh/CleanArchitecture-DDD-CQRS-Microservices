using Microsoft.EntityFrameworkCore;

namespace Assets.Service.Infrastructure.Persistence.EfCore;

public sealed class ReadDbContext : AssetsDbContextBase
{
    public ReadDbContext(DbContextOptions<ReadDbContext> options) : base(options) { }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);

        base.OnConfiguring(optionsBuilder);
    }
}


