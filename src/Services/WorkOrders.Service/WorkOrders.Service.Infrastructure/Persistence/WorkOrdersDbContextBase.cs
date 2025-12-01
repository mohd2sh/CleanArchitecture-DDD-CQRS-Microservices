using WorkOrders.Service.Domain.WorkOrders;
using WorkOrders.Service.Infrastructure.Persistence.EfCore;
using Microsoft.EntityFrameworkCore;

namespace WorkOrders.Service.Infrastructure.Persistence;

public abstract class WorkOrdersDbContextBase : DbContext
{
    protected WorkOrdersDbContextBase(DbContextOptions options) : base(options) { }

    internal DbSet<WorkOrder> WorkOrders => Set<WorkOrder>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(WorkOrdersDbContextBase).Assembly);
        modelBuilder.ApplyModelConventions();
    }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.ApplyGlobalConventions();
    }
}







