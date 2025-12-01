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
        // Register Saga DbContext
        services.AddDbContext<SagaDbContext>(opt =>
        {
            opt.UseSqlServer(
                config.GetConnectionString("SagaDb") ?? config.GetConnectionString("WriteDb")!,
                sql => sql.MigrationsAssembly(typeof(SagaDbContext).Assembly.FullName));
        });

        // Configure Quartz scheduler (required for MassTransit.Quartz)
        // This registers ISchedulerFactory and other Quartz dependencies
        services.AddQuartz(q =>
        {
            // Use a simple in-memory scheduler for now
            // Can be configured with database persistence later if needed
        });

        // Add Quartz hosted service to run the scheduler
        services.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);

        // Register MassTransit messaging (for microservices)
        var rabbitMqHost = config.GetConnectionString("RabbitMQ") ?? "rabbitmq://localhost";
        services.AddMassTransit(x =>
        {
            // Add Quartz consumers for message scheduling
            x.AddQuartzConsumers();

            // Register CompleteWorkOrder saga state machine with Entity Framework repository
            x.AddSagaStateMachine<CompleteWorkOrderSaga, CompleteWorkOrderSagaState>()
                .EntityFrameworkRepository(r =>
                {
                    r.ConcurrencyMode = ConcurrencyMode.Pessimistic;
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
                    r.ConcurrencyMode = ConcurrencyMode.Pessimistic;
                    r.AddDbContext<SagaDbContext, SagaDbContext>((provider, builder) =>
                    {
                        var configuration = provider.GetRequiredService<IConfiguration>();
                        var connectionString = configuration.GetConnectionString("SagaDb")
                            ?? configuration.GetConnectionString("WriteDb")
                            ?? throw new InvalidOperationException("Connection string 'SagaDb' or 'WriteDb' must be configured");
                        builder.UseSqlServer(connectionString);
                    });
                });

            // Configure RabbitMQ
            x.UsingRabbitMq((context, cfg) =>
            {
                cfg.Host(rabbitMqHost, h =>
                {
                    h.Username("guest");
                    h.Password("guest");
                });

                // Configure Quartz message scheduler for scheduled messages (saga timeouts)
                // This enables Schedule() to work in sagas
                // The Quartz endpoint is automatically configured by AddQuartzConsumers()
                cfg.UseMessageScheduler(new Uri("queue:quartz"));

                // Explicitly configure saga endpoints - sagas require explicit endpoint configuration
                // ConfigureEndpoints does NOT automatically create endpoints for sagas
                // ConfigureSaga<TState>() automatically discovers the state machine from registered sagas

                // Configure AssignTechnician saga endpoint
                cfg.ReceiveEndpoint("assign-technician-saga", e =>
                {
                    // Allow multiple messages for different saga instances
                    // But limit to prevent too many unacknowledged messages
                    e.PrefetchCount = 10;

                    // Configure saga (Pessimistic concurrency mode ensures same instance is sequential)
                    // Message scheduler is configured at bus level (Quartz)
                    e.ConfigureSaga<AssignTechnicianSagaState>(context);

                    // Configure retry policy with exponential backoff to prevent concurrent retries
                    e.UseMessageRetry(r =>
                    {
                        r.Ignore(typeof(DomainException));
                        r.Ignore(typeof(CleanArchitecture.Core.Application.Abstractions.Common.ApplicationException));
                        // Use exponential backoff to space out retries
                        // 3 retries: 2s, 4s, 8s (with max 30s, base 2s)
                        r.Exponential(1, TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(120), TimeSpan.FromSeconds(2));
                    });

                    // Add logging to verify endpoint creation
                    var logger = context.GetRequiredService<ILoggerFactory>().CreateLogger("Orchestration.Service");
                    logger.LogInformation(
                        "[MASS TRANSIT] Configured receive endpoint 'assign-technician-saga' for AssignTechnicianSaga with PrefetchCount=10 and exponential retry");
                    logger.LogInformation(
                        "[MASS TRANSIT] Saga endpoint should automatically bind to events: WorkOrderAssignedEvent, TechnicianAssignmentValidatedEvent, TechnicianAssignmentFailedEvent, Fault<TechnicianAssignedEvent>");
                });

                // Configure CompleteWorkOrder saga endpoint
                cfg.ReceiveEndpoint("complete-workorder-saga", e =>
                {
                    // Allow multiple messages for different saga instances
                    // But limit to prevent too many unacknowledged messages
                    e.PrefetchCount = 10;

                    // Configure saga (Pessimistic concurrency mode ensures same instance is sequential)
                    // Message scheduler is configured at bus level (Quartz)
                    e.ConfigureSaga<CompleteWorkOrderSagaState>(context);

                    // Configure retry policy with exponential backoff to prevent concurrent retries
                    e.UseMessageRetry(r =>
                    {
                        r.Ignore(typeof(DomainException));
                        r.Ignore(typeof(CleanArchitecture.Core.Application.Abstractions.Common.ApplicationException));
                        // Use exponential backoff to space out retries
                        // 3 retries: 2s, 4s, 8s (with max 30s, base 2s)
                        r.Exponential(1, TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(120), TimeSpan.FromSeconds(2));
                    });

                    // Add logging to verify endpoint creation
                    var logger = context.GetRequiredService<ILoggerFactory>().CreateLogger("Orchestration.Service");
                    logger.LogInformation(
                        "[MASS TRANSIT] Configured receive endpoint 'complete-workorder-saga' for CompleteWorkOrderSaga with PrefetchCount=10 and exponential retry");
                });

                // Configure other endpoints automatically (consumers, etc.)
                cfg.ConfigureEndpoints(context);

                // Configure message retry with exception filtering
                cfg.UseMessageRetry(r =>
                {
                    // Skip retries for domain/application exceptions (business rule violations)
                    r.Ignore(typeof(DomainException));
                    r.Ignore(typeof(CleanArchitecture.Core.Application.Abstractions.Common.ApplicationException));

                    // Retry 3 times for general exceptions (transient failures like database timeouts)
                    r.Interval(1, TimeSpan.FromSeconds(120));
                });
            });
        });


        return services;
    }
}

