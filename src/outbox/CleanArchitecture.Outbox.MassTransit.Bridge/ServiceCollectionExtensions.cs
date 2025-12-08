using CleanArchitecture.Outbox.Abstractions;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace CleanArchitecture.Outbox.MassTransit.Bridge;

/// <summary>
/// Extension methods for registering MassTransit messaging components.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds MassTransit with RabbitMQ and registers IOutboxPublisher implementation.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="config">The configuration to read RabbitMQ settings from.</param>
    /// <param name="rabbitMqHost">The RabbitMQ host connection string (e.g., "rabbitmq://localhost").</param>
    /// <param name="configure">Optional additional MassTransit configuration.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddRabbitMqMassTransitWithOutbox(
        this IServiceCollection services,
        IConfiguration config,
        string rabbitMqHost,
        Action<IBusRegistrationConfigurator>? configure = null)
    {
        // Read RabbitMQ credentials from configuration with defaults
        var username = config["RabbitMQ:Username"] ?? "guest";
        var password = config["RabbitMQ:Password"] ?? "guest";

        services.AddMassTransit(x =>
        {
            // Configure RabbitMQ
            x.UsingRabbitMq((context, cfg) =>
            {
                cfg.Host(rabbitMqHost, h =>
                {
                    h.Username(username);
                    h.Password(password);
                });

                // Configure endpoints (discovers consumers automatically)
                cfg.ConfigureEndpoints(context);

                // Enable message retry
                cfg.UseMessageRetry(r => r.Interval(3, TimeSpan.FromSeconds(5)));
            });

            // Allow additional configuration
            configure?.Invoke(x);
        });

        // Register IOutboxPublisher with MassTransit implementation
        services.TryAddScoped<IOutboxPublisher, MassTransitOutboxPublisher>();

        return services;
    }

    /// <summary>
    /// Adds integration event consumers for specified event types.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="eventTypes">The event types to register consumers for.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddIntegrationEventConsumers(
        this IServiceCollection services,
        params Type[] eventTypes)
    {
        foreach (var eventType in eventTypes)
        {
            var consumerType = typeof(IntegrationEventConsumer<>).MakeGenericType(eventType);
            services.AddScoped(consumerType);
        }

        return services;
    }
}

