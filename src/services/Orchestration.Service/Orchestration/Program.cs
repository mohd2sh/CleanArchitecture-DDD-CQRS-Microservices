using System.Globalization;
using Microsoft.EntityFrameworkCore;
using Orchestration.Service.Persistence;
using Serilog;

namespace Orchestration.Service;

public class Program
{
    public static async Task Main(string[] args)
    {
        Log.Logger = new LoggerConfiguration()
            .WriteTo.Console(formatProvider: CultureInfo.InvariantCulture)
            .CreateBootstrapLogger();

        try
        {
            Log.Information("Starting up Orchestration Service");

            var builder = Host.CreateApplicationBuilder(args);
            //TODO
            builder.Logging.AddSerilog();

            builder.Services.AddOrchestration(builder.Configuration);

            var host = builder.Build();

            // Apply database migrations in development
            if (host.Services.GetRequiredService<IHostEnvironment>().IsDevelopment())
            {
                using var scope = host.Services.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<SagaDbContext>();

                await dbContext.Database.MigrateAsync();
                Log.Information("Saga database migrations applied");
            }

            Log.Information("Orchestration Service started successfully");

            await host.RunAsync();
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Orchestration Service terminated unexpectedly");
        }
        finally
        {
            await Log.CloseAndFlushAsync();
        }
    }
}

