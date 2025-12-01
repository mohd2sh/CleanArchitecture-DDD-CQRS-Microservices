using Technicians.Service.Domain.Technicians;
using Technicians.Service.Domain.Technicians.ValueObjects;
using Technicians.Service.Infrastructure.Persistence.EfCore;
using Microsoft.EntityFrameworkCore;

namespace Technicians.Service.Infrastructure.Persistence;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(WriteDbContext context, CancellationToken ct = default)
    {
        await context.Database.EnsureCreatedAsync(ct);

        if (!await context.Technicians.AnyAsync(ct))
        {
            var technicians = new List<Technician>
            {
                Technician.Create("John Doe", SkillLevel.Journeyman),
                Technician.Create("Jane Smith", SkillLevel.Master),
                Technician.Create("Tom Wilson", SkillLevel.Apprentice)
            };

            technicians[0].AddCertification(Certification.Create("CERT-001", DateTime.UtcNow, DateTime.UtcNow.AddDays(365)));

            await context.Technicians.AddRangeAsync(technicians, ct);
            await context.SaveChangesAsync(ct);
        }
    }
}


