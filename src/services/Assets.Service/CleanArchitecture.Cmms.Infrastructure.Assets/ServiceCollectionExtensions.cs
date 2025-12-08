using System.Data;
using CleanArchitecture.Cmms.Contracts.WorkOrders.Events;
using CleanArchitecture.Cmms.Infrastructure.Assets.Common;
using CleanArchitecture.Cmms.Infrastructure.Assets.Persistence.EfCore;
using CleanArchitecture.Cmms.Infrastructure.Assets.Repositories.ReadRepositories;
using CleanArchitecture.Cmms.Infrastructure.Assets.Repositories.WriteRepositories;
using CleanArchitecture.Core.Application.Abstractions.Common;
using CleanArchitecture.Core.Application.Abstractions.Persistence;
using CleanArchitecture.Core.Application.Abstractions.Persistence.Repositories;
using CleanArchitecture.Core.Infrastructure;
using CleanArchitecture.Core.Infrastructure.Persistence.EfCore.Interceptors;
using CleanArchitecture.Outbox;
using CleanArchitecture.Outbox.MassTransit.Bridge;
using MassTransit;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CleanArchitecture.Cmms.Infrastructure.Assets;

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

        // Register MassTransit messaging (for microservices)
        var rabbitMqHost = config.GetConnectionString("RabbitMQ") ?? "rabbitmq://localhost";
        services.AddRabbitMqMassTransitWithOutbox(rabbitMqHost, x =>
        {
            x.SetEndpointNameFormatter(new KebabCaseEndpointNameFormatter("assets", false));

            // Register consumers for integration events from WorkOrders service
            x.AddConsumer<IntegrationEventConsumer<WorkOrderCreatedEvent>>();

            // Note: WorkOrderCompletedEvent is now handled by Saga, but we keep the consumer
            // as a fallback for backward compatibility or direct event handling
            x.AddConsumer<IntegrationEventConsumer<WorkOrderCompletedEvent>>();

            // Register consumer for WorkOrderCompletionRequestedEvent from Saga orchestrator
            x.AddConsumer<IntegrationEventConsumer<WorkOrderCompletionRequestedEvent>>();

            // Register command consumers for Saga orchestration (kept for backward compatibility)
            //x.AddConsumer<CompleteAssetMaintenanceCommandHandler>();

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
            return new CleanArchitecture.Core.Infrastructure.Persistence.EfCore.EfUnitOfWork(sp.GetRequiredService<WriteDbContext>());
        });
    }

    private static void AddReadDbServices(IServiceCollection services, IConfiguration config, string environment)
    {
        // For queries use IReadRepository using EF ReadDbContext
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
    }
}

