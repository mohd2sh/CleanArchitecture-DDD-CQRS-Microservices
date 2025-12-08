using System.Data;
using CleanArchitecture.Cmms.Contracts.Technicians.Events;
using CleanArchitecture.Cmms.Contracts.WorkOrders.Events;
using CleanArchitecture.Cmms.Infrastructure.Technicians.Common;
using CleanArchitecture.Cmms.Infrastructure.Technicians.Persistence.EfCore;
using CleanArchitecture.Cmms.Infrastructure.Technicians.Repositories.ReadRepositories;
using CleanArchitecture.Cmms.Infrastructure.Technicians.Repositories.WriteRepositories;
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
using WorkOrderCompletionRequestedEvent = CleanArchitecture.Cmms.Contracts.WorkOrders.Events.WorkOrderCompletionRequestedEvent;

namespace CleanArchitecture.Cmms.Infrastructure.Technicians;

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
            x.SetEndpointNameFormatter(new KebabCaseEndpointNameFormatter("technicians", false));

            // Register consumers for integration events from WorkOrders service
            x.AddConsumer<IntegrationEventConsumer<TechnicianAssignedEvent>>();
            x.AddConsumer<IntegrationEventConsumer<TechnicianUnAssignedEvent>>();

            // Note: WorkOrderCompletedEvent is now handled by Saga, but we keep the consumer
            // as a fallback for backward compatibility or direct event handling
            x.AddConsumer<IntegrationEventConsumer<WorkOrderCompletedEvent>>();

            // Register consumer for WorkOrderCompletionRequestedEvent from Saga orchestrator
            x.AddConsumer<IntegrationEventConsumer<WorkOrderCompletionRequestedEvent>>();

            x.AddConsumer<IntegrationEventConsumer<CompleteTechnicianAssignmentRequestedEvent>>();
            x.AddConsumer<IntegrationEventConsumer<RevertTechnicianAssignmentRequestedEvent>>();
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

