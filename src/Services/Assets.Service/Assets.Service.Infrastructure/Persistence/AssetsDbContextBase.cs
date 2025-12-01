using Assets.Service.Domain.Assets;
using Assets.Service.Infrastructure.Persistence.EfCore;
using Microsoft.EntityFrameworkCore;

namespace Assets.Service.Infrastructure.Persistence;

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


