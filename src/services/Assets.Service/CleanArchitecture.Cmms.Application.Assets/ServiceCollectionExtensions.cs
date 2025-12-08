using CleanArchitecture.Core.Application;
using CleanArchitecture.Core.Application.Abstractions.Events;
using CleanArchitecture.Core.Application.Pipelines;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace CleanArchitecture.Cmms.Application.Assets;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = typeof(ServiceCollectionExtensions).Assembly;

        // Register Command & Query Handlers, Domain Event Handlers, and Integration Event Handlers
        services.AddApplicationHandlers(assembly);

        // Register Pipelines (order matters, so user controls registration)
        AddPipelines(services);

        // Fluent Validation
        services.AddValidatorsFromAssembly(assembly, includeInternalTypes: true);

        return services;
    }

    private static void AddPipelines(IServiceCollection services)
    {
        // Generic pipeline - runs for both commands and queries
        services.AddScoped(typeof(IPipeline<,>), typeof(LoggingPipeline<,>));
        services.AddScoped(typeof(IPipeline<,>), typeof(ValidationPipeline<,>));

        // Command-specific pipelines - run only for commands
        services.AddScoped(typeof(ICommandPipeline<,>), typeof(TransactionCommandPipeline<,>));
        services.AddScoped(typeof(ICommandPipeline<,>), typeof(DomainEventsPipeline<,>));

        // Integration event pipelines - run for integration event handlers
        // Order matters: TransactionIntegrationEventPipeline (outermost - wraps in transaction)
        // then DomainEventsIntegrationEventPipeline (collects domain events, writes to outbox)
        services.AddScoped(typeof(IIntegrationEventPipeline<>), typeof(TransactionIntegrationEventPipeline<>));
        services.AddScoped(typeof(IIntegrationEventPipeline<>), typeof(DomainEventsIntegrationEventPipeline<>));
    }
}

