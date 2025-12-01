using Microsoft.EntityFrameworkCore;
using Orchestration.Service.WorkOrder.Sagas;

namespace Orchestration.Service.Persistence;

/// <summary>
/// DbContext for saga state persistence.
/// </summary>
public class SagaDbContext : DbContext
{
    public SagaDbContext(DbContextOptions<SagaDbContext> options) : base(options) { }

    public DbSet<CompleteWorkOrderSagaState> CompleteWorkOrderSagaStates { get; set; } = null!;
    public DbSet<AssignTechnicianSagaState> AssignTechnicianSagaStates { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<CompleteWorkOrderSagaState>(entity =>
        {
            entity.ToTable("CompleteWorkOrderSagaStates", "sagas");
            entity.HasKey(x => x.CorrelationId);
            entity.Property(x => x.CurrentState).HasMaxLength(64);
        });

        modelBuilder.Entity<AssignTechnicianSagaState>(entity =>
        {
            entity.ToTable("AssignTechnicianSagaStates", "sagas");
            entity.HasKey(x => x.CorrelationId);
            entity.Property(x => x.CurrentState).HasMaxLength(64);
        });

        base.OnModelCreating(modelBuilder);
    }
}

