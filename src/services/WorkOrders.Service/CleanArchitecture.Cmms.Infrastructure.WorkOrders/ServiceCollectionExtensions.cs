using System.Data;
using CleanArchitecture.Cmms.Application.WorkOrders.Interfaces;
using CleanArchitecture.Cmms.Contracts.WorkOrders.Events;
using CleanArchitecture.Cmms.Infrastructure.WorkOrders.Common;
using CleanArchitecture.Cmms.Infrastructure.WorkOrders.Persistence.EfCore;
using CleanArchitecture.Cmms.Infrastructure.WorkOrders.Repositories.ReadRepositories;
using CleanArchitecture.Cmms.Infrastructure.WorkOrders.Repositories.ReadRepositories.WorkOrders;
using CleanArchitecture.Cmms.Infrastructure.WorkOrders.Repositories.WriteRepositories;
using CleanArchitecture.Core.Application.Abstractions.Common;
using CleanArchitecture.Core.Application.Abstractions.Persistence;
using CleanArchitecture.Core.Application.Abstractions.Persistence.Repositories;
using CleanArchitecture.Core.Infrastructure;
using CleanArchitecture.Core.Infrastructure.Persistence.EfCore;
using CleanArchitecture.Core.Infrastructure.Persistence.EfCore.Interceptors;
using CleanArchitecture.Outbox;
using CleanArchitecture.Outbox.MassTransit.Bridge;
using MassTransit;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CleanArchitecture.Cmms.Infrastructure.WorkOrders;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration config,
        string environment)
    {
        AddWriteDbServices(services, config);
        AddReadDbServices(services, config, environment);

        // Register Core Infrastructure services (Mediator, Event Dispatchers)
        services.AddCoreInfrastructure();

        var rabbitMqHost = config.GetConnectionString("RabbitMQ") ?? "rabbitmq://localhost";
        services.AddRabbitMqMassTransitWithOutbox(config, rabbitMqHost, x =>
        {
            x.SetEndpointNameFormatter(new KebabCaseEndpointNameFormatter("workorders", false));

            // Register consumer for compensation event from Saga orchestrator
            x.AddConsumer<IntegrationEventConsumer<WorkOrderCompletingFailedEvent>>();
        });

        // Register Outbox (will use MassTransitOutboxPublisher)
        var outboxConnectionString = config.GetConnectionString("WriteDb")!;
        services.AddOutbox(outboxConnectionString);

        services.AddSingleton<IDateTimeProvider, SystemDateTimeProvider>();

        return services;
    }

    private static void AddWriteDbServices(IServiceCollection services, IConfiguration config)
    {
        services.AddScoped<AuditableEntityInterceptor>(sp =>
        {
            var dateTimeProvider = sp.GetRequiredService<IDateTimeProvider>();

            // Replace with actual user resolution logic as needed (e.g., from the current HTTP context)
            var resolveCurrentUserFunc = () => { return "System"; };

            return new AuditableEntityInterceptor(dateTimeProvider, resolveCurrentUserFunc);
        });

        services.AddDbContext<WriteDbContext>((sp, opt) =>
        {
            var interceptor = sp.GetRequiredService<AuditableEntityInterceptor>();

            opt.AddInterceptors(interceptor);

            opt.UseSqlServer(
                config.GetConnectionString("WriteDb"),
                sql => sql.MigrationsAssembly(typeof(WriteDbContext).Assembly.FullName));
        });

        services.AddScoped(typeof(IRepository<,>), typeof(EfRepository<,>));

        services.AddScoped<IUnitOfWork>(sp =>
        {
            return new EfUnitOfWork(sp.GetRequiredService<WriteDbContext>());
        });
    }

    private static void AddReadDbServices(IServiceCollection services, IConfiguration config, string environment)
    {
        // For queries use IReadRepository using Dapper + EF ReadDbContext
        services.AddDbContext<ReadDbContext>(opt =>
        {
            opt.UseSqlServer(
                config.GetConnectionString("ReadDb") ?? config.GetConnectionString("WriteDb"),
                sql => sql.MigrationsAssembly(typeof(ReadDbContext).Assembly.FullName));

            opt.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);

            if (environment == "Development")
                opt.LogTo(Console.WriteLine, LogLevel.Information);
        });

        services.AddScoped(typeof(IReadRepository<,>), typeof(EfReadRepository<,>));

        services.AddTransient<IDbConnection>(sp =>
        {
            var connectionString = config.GetConnectionString("ReadDb")
                ?? config.GetConnectionString("WriteDb");
            return new SqlConnection(connectionString);
        });

        services.AddScoped<IWorkOrderReadRepository, WorkOrderReadRepository>();
    }
}

