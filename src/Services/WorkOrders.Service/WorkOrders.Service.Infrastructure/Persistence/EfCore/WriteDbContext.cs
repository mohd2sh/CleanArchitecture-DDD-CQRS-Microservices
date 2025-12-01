using Microsoft.EntityFrameworkCore;

namespace WorkOrders.Service.Infrastructure.Persistence.EfCore;

public sealed class WriteDbContext : WorkOrdersDbContextBase
{
    public WriteDbContext(DbContextOptions<WriteDbContext> options) : base(options) { }
}







