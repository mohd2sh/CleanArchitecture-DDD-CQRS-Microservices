using Microsoft.EntityFrameworkCore;

namespace CleanArchitecture.Cmms.Infrastructure.Technicians.Persistence.EfCore;

public sealed class ReadDbContext : TechniciansDbContextBase
{
    public ReadDbContext(DbContextOptions<ReadDbContext> options) : base(options) { }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);

        base.OnConfiguring(optionsBuilder);
    }
}

