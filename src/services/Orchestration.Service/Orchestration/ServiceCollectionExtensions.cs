using CleanArchitecture.Core.Domain.Abstractions;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Orchestration.Service.WorkOrder.Sagas;
using Quartz;
using SagaDbContext = Orchestration.Service.Persistence.SagaDbContext;

namespace Orchestration.Service;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddOrchestration(
        this IServiceCollection services,
        IConfiguration config)
    {
        services.AddDbContext<SagaDbContext>(opt =>
        {
            opt.UseSqlServer(
                config.GetConnectionString("SagaDb") ?? config.GetConnectionString("WriteDb")!,
                sql => sql.MigrationsAssembly(typeof(SagaDbContext).Assembly.FullName));
        });

        services.AddQuartz(q =>
        {
            // Use a simple in-memory scheduler for now
        });

        services.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);

        // Register MassTransit messaging (for microservices)
        var rabbitMqHost = config.GetConnectionString("RabbitMQ") ?? "rabbitmq://localhost";
        services.AddMassTransit(x =>
        {
            x.AddQuartzConsumers();

            // Register CompleteWorkOrder saga state machine with Entity Framework repository
            x.AddSagaStateMachine<CompleteWorkOrderSaga, CompleteWorkOrderSagaState>()
                .EntityFrameworkRepository(r =>
                {
                    r.ConcurrencyMode = ConcurrencyMode.Optimistic;
                    r.AddDbContext<SagaDbContext, SagaDbContext>((provider, builder) =>
                    {
                        var configuration = provider.GetRequiredService<IConfiguration>();
                        var connectionString = configuration.GetConnectionString("SagaDb")
                            ?? configuration.GetConnectionString("WriteDb")
                            ?? throw new InvalidOperationException("Connection string 'SagaDb' or 'WriteDb' must be configured");
                        builder.UseSqlServer(connectionString);
                    });
                });

            // Register AssignTechnician saga state machine with Entity Framework repository
            x.AddSagaStateMachine<AssignTechnicianSaga, AssignTechnicianSagaState>()
                .EntityFrameworkRepository(r =>
                {
                    r.ConcurrencyMode = ConcurrencyMode.Optimistic;
                    r.AddDbContext<SagaDbContext, SagaDbContext>((provider, builder) =>
                    {
                        var configuration = provider.GetRequiredService<IConfiguration>();
                        var connectionString = configuration.GetConnectionString("SagaDb")
                            ?? configuration.GetConnectionString("WriteDb")
                            ?? throw new InvalidOperationException("Connection string 'SagaDb' or 'WriteDb' must be configured");
                        builder.UseSqlServer(connectionString);
                    });
                });

            x.UsingRabbitMq((context, cfg) =>
            {
                var username = config["RabbitMQ:Username"] ?? "guest";
                var password = config["RabbitMQ:Password"] ?? "guest";

                cfg.Host(rabbitMqHost, h =>
                {
                    h.Username(username);
                    h.Password(password);
                });

                cfg.UseMessageScheduler(new Uri("queue:quartz"));

                // Configure AssignTechnician saga endpoint
                cfg.ReceiveEndpoint("assign-technician-saga", e =>
                {
                    e.PrefetchCount = 10;

                    e.ConfigureSaga<AssignTechnicianSagaState>(context);

                    e.UseMessageRetry(r =>
                    {
                        r.Ignore(typeof(DomainException));
                        r.Ignore(typeof(CleanArchitecture.Core.Application.Abstractions.Common.ApplicationException));
                        r.Exponential(1, TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(30), TimeSpan.FromSeconds(2));
                    });

                    // Add logging to verify endpoint creation
                    var logger = context.GetRequiredService<ILoggerFactory>().CreateLogger("Orchestration.Service");
                    logger.LogInformation(
                        "[MASS TRANSIT] Configured receive endpoint 'assign-technician-saga' for AssignTechnicianSaga");
                });

                cfg.ReceiveEndpoint("complete-workorder-saga", e =>
                {
                    e.PrefetchCount = 10;

                    e.ConfigureSaga<CompleteWorkOrderSagaState>(context);

                    e.UseMessageRetry(r =>
                    {
                        r.Ignore(typeof(DomainException));
                        r.Ignore(typeof(CleanArchitecture.Core.Application.Abstractions.Common.ApplicationException));
                        r.Exponential(1, TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(30), TimeSpan.FromSeconds(2));
                    });

                    var logger = context.GetRequiredService<ILoggerFactory>().CreateLogger("Orchestration.Service");
                    logger.LogInformation(
                        "[MASS TRANSIT] Configured receive endpoint 'complete-workorder-saga' for CompleteWorkOrderSaga");
                });

                cfg.ConfigureEndpoints(context);

                cfg.UseMessageRetry(r =>
                {
                    // Skip retries for domain/application exceptions (business rule violations)
                    r.Ignore(typeof(DomainException));
                    r.Ignore(typeof(CleanArchitecture.Core.Application.Abstractions.Common.ApplicationException));

                    r.Interval(1, TimeSpan.FromSeconds(30));
                });
            });
        });

        return services;
    }
}

