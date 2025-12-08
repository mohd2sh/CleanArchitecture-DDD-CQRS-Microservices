using CleanArchitecture.Cmms.Domain.Assets;
using CleanArchitecture.Cmms.Infrastructure.Assets.Persistence.EfCore;
using Microsoft.EntityFrameworkCore;

namespace CleanArchitecture.Cmms.Infrastructure.Assets.Persistence;

public abstract class AssetsDbContextBase : DbContext
{
    protected AssetsDbContextBase(DbContextOptions options) : base(options) { }

    internal DbSet<Asset> Assets => Set<Asset>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AssetsDbContextBase).Assembly);
        modelBuilder.ApplyModelConventions();
    }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.ApplyGlobalConventions();
    }
}

