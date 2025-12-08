using System.Globalization;
using Asp.Versioning;
using Asp.Versioning.ApiExplorer;
using CleanArchitecture.Cmms.Application.Technicians;
using CleanArchitecture.Cmms.Infrastructure.Technicians;
using CleanArchitecture.Core.Api.Configurations;
using CleanArchitecture.Core.Api.Filters;
using CleanArchitecture.Core.Api.Middlewares;
using CleanArchitecture.Outbox.Persistence;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace CleanArchitecture.Cmms.Api.Technicians;

public class Program
{
    private Program() { }

    public static async Task Main(string[] args)
    {
        Log.Logger = new LoggerConfiguration()
            .WriteTo.Console(formatProvider: CultureInfo.InvariantCulture)
            .CreateBootstrapLogger();

        try
        {
            Log.Information("Starting up Technicians Service API");

            var builder = WebApplication.CreateBuilder(args);

            // Register application and infrastructure layers
            builder.Services.AddApplication();
            builder.Services.AddInfrastructure(builder.Configuration, builder.Environment.EnvironmentName);

            builder.Services.AddControllers(options =>
            {
                options.Filters.Add<ResultToHttpStatusFilter>();
            });
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            builder.Services.ConfigureOptions<ConfigureSwaggerOptions>();
            builder.Services.ConfigureOptions<ConfigureApiBehaviorOptions>();

            builder.Services.AddApiVersioning(options =>
            {
                options.DefaultApiVersion = new ApiVersion(1, 0);
                options.AssumeDefaultVersionWhenUnspecified = true;
                options.ReportApiVersions = true;
            })
            .AddApiExplorer(options =>
            {
                options.GroupNameFormat = "'v'VVV";
                options.SubstituteApiVersionInUrl = true;
            });

            builder.Services.AddTransient<ExceptionHandlingMiddleware>();

            // Health check endpoint
            builder.Services.AddHealthChecks();

            builder.Host.UseSerilog((context, services, configuration) =>
            {
                configuration
                    .ReadFrom.Configuration(context.Configuration)
                    .ReadFrom.Services(services)
                    .Enrich.FromLogContext();
            });

            var app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                using (var scope = app.Services.CreateScope())
                {
                    // Database seeding is now handled by init.sql via docker-compose
                    // await DatabaseSeeder.SeedAsync(db);

                    // Apply OutboxDbContext migrations to ensure OutboxMessages table exists
                    var outboxDb = scope.ServiceProvider.GetRequiredService<OutboxDbContext>();
                    await outboxDb.Database.MigrateAsync();
                }

                var provider = app.Services.GetRequiredService<IApiVersionDescriptionProvider>();

                app.UseSwagger();
                app.UseSwaggerUI(options =>
                {
                    foreach (var groupName in provider.ApiVersionDescriptions.Select(a => a.GroupName))
                    {
                        options.SwaggerEndpoint(
                            $"/swagger/{groupName}/swagger.json",
                            groupName.ToUpperInvariant());
                    }
                });
            }

            app.UseSerilogRequestLogging();

            app.UseMiddleware<ExceptionHandlingMiddleware>();

            // Health check endpoint
            app.MapHealthChecks("/health");

            app.MapControllers();

            Log.Information("Technicians Service API started successfully");

            await app.RunAsync();
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Application terminated unexpectedly");
        }
        finally
        {
            await Log.CloseAndFlushAsync();
        }
    }
}

