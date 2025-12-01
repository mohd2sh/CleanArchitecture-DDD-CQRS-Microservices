using Microsoft.EntityFrameworkCore;

namespace WorkOrders.Service.Infrastructure.Persistence.EfCore;

public static class ModelBuilderConventions
{
    public static void ApplyModelConventions(this ModelBuilder b)
    {
        b.HasSequence<long>("workorder_hilo").StartsAt(1000).IncrementsBy(10);
    }

    public static void ApplyGlobalConventions(this ModelConfigurationBuilder c)
    {
        c.Properties<decimal>().HavePrecision(18, 2);
    }
}







