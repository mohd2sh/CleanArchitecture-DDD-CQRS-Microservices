using CleanArchitecture.Cmms.Domain.Technicians;
using CleanArchitecture.Cmms.Infrastructure.Technicians.Persistence.EfCore;
using Microsoft.EntityFrameworkCore;

namespace CleanArchitecture.Cmms.Infrastructure.Technicians.Persistence;

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

