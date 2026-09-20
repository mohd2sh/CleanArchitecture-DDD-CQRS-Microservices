using MassTransit;
using MassTransit.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Orchestration.Service.Persistence;
using Orchestration.Service.WorkOrder.Sagas;
using Quartz;
using Testcontainers.MsSql;
using Testcontainers.RabbitMq;

namespace CleanArchitecture.Cmms.Orchestration.IntegrationTests.Infrastructure;

public abstract class OrchestrationTestBase : IAsyncLifetime
{
    protected MsSqlContainer SqlServerContainer { get; private set; } = null!;
    protected RabbitMqContainer RabbitMqContainer { get; private set; } = null!;
    protected string SagaConnectionString { get; private set; } = null!;
    protected string RabbitMqConnectionString { get; private set; } = null!;
    protected IServiceProvider ServiceProvider { get; private set; } = null!;
    protected IBusControl BusControl { get; private set; } = null!;
    protected SagaDbContext SagaDbContext { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        // Start SQL Server container for saga state
        SqlServerContainer = new MsSqlBuilder()
            .WithImage("mcr.microsoft.com/mssql/server:2022-latest")
            .WithPassword("YourStrong!Passw0rd")
            .WithEnvironment("ACCEPT_EULA", "Y")
            .WithCleanUp(true)
            .Build();

        await SqlServerContainer.StartAsync();
        SagaConnectionString = SqlServerContainer.GetConnectionString();

        // Start RabbitMQ container
        RabbitMqContainer = new RabbitMqBuilder()
            .WithImage("rabbitmq:3-management")
            .WithUsername("guest")
            .WithPassword("guest")
            .WithCleanUp(true)
            .Build();

        await RabbitMqContainer.StartAsync();
        RabbitMqConnectionString = $"amqp://guest:guest@{RabbitMqContainer.Hostname}:{RabbitMqContainer.GetMappedPublicPort(5672)}";

        // Configure services
        var services = new ServiceCollection();
        services.AddLogging(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Warning));

        // Add DbContext
        services.AddDbContext<SagaDbContext>(options =>
            options.UseSqlServer(SagaConnectionString));

        // Configure MassTransit with test harness
        services.AddMassTransitTestHarness(x =>
        {
            x.AddQuartzConsumers();

            // Register CompleteWorkOrder saga
            x.AddSagaStateMachine<CompleteWorkOrderSaga, CompleteWorkOrderSagaState>()
                .EntityFrameworkRepository(r =>
                {
                    r.ConcurrencyMode = ConcurrencyMode.Optimistic;
                    r.AddDbContext<SagaDbContext, SagaDbContext>((provider, builder) =>
                    {
                        builder.UseSqlServer(SagaConnectionString);
                    });
                });

            // Register AssignTechnician saga
            x.AddSagaStateMachine<AssignTechnicianSaga, AssignTechnicianSagaState>()
                .EntityFrameworkRepository(r =>
                {
                    r.ConcurrencyMode = ConcurrencyMode.Optimistic;
                    r.AddDbContext<SagaDbContext, SagaDbContext>((provider, builder) =>
                    {
                        builder.UseSqlServer(SagaConnectionString);
                    });
                });

            // Configure RabbitMQ
            x.UsingRabbitMq((context, cfg) =>
            {
                cfg.Host(new Uri(RabbitMqConnectionString), h =>
                {
                    h.Username("guest");
                    h.Password("guest");
                });

                cfg.UseMessageScheduler(new Uri("queue:quartz"));

                // Configure saga endpoints
                cfg.ReceiveEndpoint("assign-technician-saga", e =>
                {
                    e.ConfigureSaga<AssignTechnicianSagaState>(context);
                });

                cfg.ReceiveEndpoint("complete-work-order-saga", e =>
                {
                    e.ConfigureSaga<CompleteWorkOrderSagaState>(context);
                });

                cfg.ConfigureEndpoints(context);
            });
        });

        // Add Quartz
        services.AddQuartz();
        services.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);

        ServiceProvider = services.BuildServiceProvider();

        // Apply migrations
        using (var scope = ServiceProvider.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<SagaDbContext>();
            await dbContext.Database.EnsureCreatedAsync();
        }

        // Start bus
        var harness = ServiceProvider.GetRequiredService<ITestHarness>();
        await harness.Start();

        BusControl = ServiceProvider.GetRequiredService<IBusControl>();
        SagaDbContext = ServiceProvider.GetRequiredService<SagaDbContext>();
    }

    public async Task DisposeAsync()
    {
        var harness = ServiceProvider?.GetRequiredService<ITestHarness>();
        if (harness != null)
        {
            await harness.Stop();
        }

        if (SqlServerContainer is not null)
        {
            await SqlServerContainer.DisposeAsync();
        }

        if (RabbitMqContainer is not null)
        {
            await RabbitMqContainer.DisposeAsync();
        }
    }

    protected async Task<CompleteWorkOrderSagaState?> GetSagaStateAsync(Guid workOrderId)
    {
        return await SagaDbContext.CompleteWorkOrderSagaStates
            .FirstOrDefaultAsync(s => s.WorkOrderId == workOrderId);
    }

    protected async Task<AssignTechnicianSagaState?> GetAssignTechnicianSagaStateAsync(Guid workOrderId)
    {
        return await SagaDbContext.AssignTechnicianSagaStates
            .FirstOrDefaultAsync(s => s.WorkOrderId == workOrderId);
    }
}

