using Microsoft.EntityFrameworkCore;

namespace CleanArchitecture.Cmms.Infrastructure.Assets.Persistence.EfCore;

public sealed class WriteDbContext : AssetsDbContextBase
{
    public WriteDbContext(DbContextOptions<WriteDbContext> options) : base(options) { }
}

