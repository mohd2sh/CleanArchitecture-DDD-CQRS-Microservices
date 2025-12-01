using Microsoft.EntityFrameworkCore;

namespace Assets.Service.Infrastructure.Persistence.EfCore;

public sealed class WriteDbContext : AssetsDbContextBase
{
    public WriteDbContext(DbContextOptions<WriteDbContext> options) : base(options) { }
}


