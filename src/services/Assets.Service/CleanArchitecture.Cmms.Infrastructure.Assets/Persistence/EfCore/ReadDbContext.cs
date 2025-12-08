using Microsoft.EntityFrameworkCore;

namespace CleanArchitecture.Cmms.Infrastructure.Assets.Persistence.EfCore;

public sealed class ReadDbContext : AssetsDbContextBase
{
    public ReadDbContext(DbContextOptions<ReadDbContext> options) : base(options) { }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);

        base.OnConfiguring(optionsBuilder);
    }
}

