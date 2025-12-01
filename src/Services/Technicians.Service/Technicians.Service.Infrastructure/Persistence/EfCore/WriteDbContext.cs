using Microsoft.EntityFrameworkCore;

namespace Technicians.Service.Infrastructure.Persistence.EfCore;

public sealed class WriteDbContext : TechniciansDbContextBase
{
    public WriteDbContext(DbContextOptions<WriteDbContext> options) : base(options) { }
}


