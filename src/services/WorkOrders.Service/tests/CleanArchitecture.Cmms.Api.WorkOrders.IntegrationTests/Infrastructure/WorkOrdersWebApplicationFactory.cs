using System.Data;
using CleanArchitecture.Cmms.Infrastructure.WorkOrders.Persistence.EfCore;
using CleanArchitecture.Outbox.Persistence;
using DotNet.Testcontainers.Builders;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Testcontainers.MsSql;

namespace CleanArchitecture.Cmms.Api.WorkOrders.IntegrationTests.Infrastructure;

public sealed class WorkOrdersWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly MsSqlContainer _sqlServerContainer;
    private readonly SemaphoreSlim _initializationLock = new(1, 1);
    private bool _isInitialized;

    public WorkOrdersWebApplicationFactory()
    {
        _sqlServerContainer = new MsSqlBuilder()
            .WithImage("mcr.microsoft.com/mssql/server:2022-latest")
            .WithPassword("YourStrong!Passw0rd")
            .WithWaitStrategy(Wait.ForUnixContainer().UntilDatabaseIsAvailable(SqlClientFactory.Instance))
            .WithEnvironment("ACCEPT_EULA", "Y")
            .WithCleanUp(true)
            .Build();
    }

    public string ConnectionString => _sqlServerContainer.GetConnectionString();

    protected override IHost CreateHost(IHostBuilder builder)
    {
        // Ensure container is started before host building begins
        EnsureContainerStartedAsync().GetAwaiter().GetResult();
        return base.CreateHost(builder);
    }

    private async Task EnsureContainerStartedAsync()
    {
        if (_isInitialized)
        {
            return;
        }

        await _initializationLock.WaitAsync();
        try
        {
            if (!_isInitialized)
            {
                await _sqlServerContainer.StartAsync();
                _isInitialized = true;
            }
        }
        finally
        {
            _initializationLock.Release();
        }
    }

    public async Task InitializeAsync()
    {
        // Ensure container is started (idempotent)
        await EnsureContainerStartedAsync();

        var options = new DbContextOptionsBuilder<WriteDbContext>()
            .UseSqlServer(ConnectionString)
            .Options;

        var outBoxOptions = new DbContextOptionsBuilder<OutboxDbContext>()
            .UseSqlServer(ConnectionString)
            .Options;

        // Ensure EF Core migrations are applied once per test run
        // Note: DbContexts are created directly with options, no need for Services scope
        using var writeDb = new WriteDbContext(options);
        using var outboxDb = new OutboxDbContext(outBoxOptions);

        await writeDb.Database.EnsureCreatedAsync();
        await outboxDb.Database.MigrateAsync();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureAppConfiguration((context, config) =>
        {
            var connectionString = ConnectionString;

            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "ConnectionStrings:WriteDb", connectionString },
                { "ConnectionStrings:ReadDb", connectionString },
                { "ConnectionStrings:RabbitMQ", "rabbitmq://localhost" }
            });
        });

        builder.ConfigureServices(services =>
        {
            RemoveDbContext<WriteDbContext>(services);
            RemoveDbContext<ReadDbContext>(services);
            RemoveDbContext<OutboxDbContext>(services);
            RemoveService<IDbConnection>(services);
            RemoveService<SqlConnection>(services);

            var connectionString = ConnectionString;

            services.AddDbContext<WriteDbContext>(o =>
                o.UseSqlServer(connectionString).EnableSensitiveDataLogging());

            services.AddDbContext<ReadDbContext>(o =>
                o.UseSqlServer(connectionString)
                 .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking)
                 .EnableSensitiveDataLogging());

            services.AddDbContext<OutboxDbContext>(o =>
                o.UseSqlServer(connectionString)
                 .EnableSensitiveDataLogging());

            services.AddTransient<IDbConnection>(_ => new SqlConnection(connectionString));

            RemoveHostedService<CleanArchitecture.Outbox.Processing.OutboxProcessor>(services);

            // Remove MassTransit services to prevent RabbitMQ connection attempts in tests
            RemoveMassTransitServices(services);
        });
    }

    private static void RemoveService<T>(IServiceCollection services)
    {
        var descriptors = services.Where(d => d.ServiceType == typeof(T)).ToList();
        foreach (var descriptor in descriptors)
        {
            services.Remove(descriptor);
        }
    }

    private static void RemoveDbContext<T>(IServiceCollection services) where T : DbContext
    {
        var descriptors = services.Where(d =>
            d.ServiceType == typeof(T) ||
            d.ServiceType == typeof(DbContextOptions<T>) ||
            d.ServiceType == typeof(DbContextOptions)).ToList();
        foreach (var descriptor in descriptors)
        {
            services.Remove(descriptor);
        }
    }

    private static void RemoveHostedService<THosted>(IServiceCollection services)
    {
        var descriptors = services
            .Where(d => d.ServiceType == typeof(IHostedService)
                && d.ImplementationType == typeof(THosted))
            .ToList();

        foreach (var descriptor in descriptors)
        {
            services.Remove(descriptor);
        }
    }

    private static void RemoveMassTransitServices(IServiceCollection services)
    {
        // Remove MassTransit hosted services (bus control)
        var hostedServiceDescriptors = services
            .Where(d => d.ServiceType == typeof(IHostedService)
                && d.ImplementationType != null
                && d.ImplementationType.FullName != null
                && (d.ImplementationType.FullName.Contains("MassTransit", StringComparison.OrdinalIgnoreCase)
                    || d.ImplementationType.FullName.Contains("BusControl", StringComparison.OrdinalIgnoreCase)))
            .ToList();

        foreach (var descriptor in hostedServiceDescriptors)
        {
            services.Remove(descriptor);
        }

        // Remove MassTransit bus interfaces if registered (using reflection to avoid package dependency)
        var busType = Type.GetType("MassTransit.IBus, MassTransit");
        var busControlType = Type.GetType("MassTransit.IBusControl, MassTransit");

        if (busType != null)
        {
            var descriptors = services.Where(d => d.ServiceType == busType).ToList();
            foreach (var descriptor in descriptors)
            {
                services.Remove(descriptor);
            }
        }

        if (busControlType != null)
        {
            var descriptors = services.Where(d => d.ServiceType == busControlType).ToList();
            foreach (var descriptor in descriptors)
            {
                services.Remove(descriptor);
            }
        }
    }

    public new async Task DisposeAsync()
    {
        await _sqlServerContainer.DisposeAsync();
        await base.DisposeAsync();
    }
}

