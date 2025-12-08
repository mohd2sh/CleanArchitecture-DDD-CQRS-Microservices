using Microsoft.EntityFrameworkCore;

namespace CleanArchitecture.Cmms.Infrastructure.Technicians.Persistence.EfCore;

public sealed class WriteDbContext : TechniciansDbContextBase
{
    public WriteDbContext(DbContextOptions<WriteDbContext> options) : base(options) { }
}

