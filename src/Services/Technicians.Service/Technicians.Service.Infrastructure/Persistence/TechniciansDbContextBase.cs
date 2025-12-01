using Technicians.Service.Domain.Technicians;
using Technicians.Service.Infrastructure.Persistence.EfCore;
using Microsoft.EntityFrameworkCore;

namespace Technicians.Service.Infrastructure.Persistence;

public abstract class TechniciansDbContextBase : DbContext
{
    protected TechniciansDbContextBase(DbContextOptions options) : base(options) { }

    internal DbSet<Technician> Technicians => Set<Technician>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(TechniciansDbContextBase).Assembly);
        modelBuilder.ApplyModelConventions();
    }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.ApplyGlobalConventions();
    }
}


