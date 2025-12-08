using CleanArchitecture.Cmms.Domain.Technicians;
using CleanArchitecture.Cmms.Domain.Technicians.ValueObjects;
using CleanArchitecture.Cmms.Infrastructure.Technicians.Persistence.EfCore;
using Microsoft.EntityFrameworkCore;

namespace CleanArchitecture.Cmms.Infrastructure.Technicians.Persistence;

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

