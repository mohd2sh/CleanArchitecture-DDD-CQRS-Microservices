using CleanArchitecture.Core.Application.Abstractions.Messaging;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace CleanArchitecture.Outbox.MassTransit.Bridge;

/// <summary>
/// Generic MassTransit consumer that bridges commands from RabbitMQ to IMediator.
/// This allows command handlers to work unchanged in microservices architecture.
/// </summary>
/// <typeparam name="TCommand">The type of command.</typeparam>
public sealed class CommandConsumer<TCommand> : IConsumer<TCommand>
    where TCommand : class
{
    private readonly IMediator _mediator;
    private readonly ILogger<CommandConsumer<TCommand>> _logger;

    public CommandConsumer(
        IMediator mediator,
        ILogger<CommandConsumer<TCommand>> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<TCommand> context)
    {
        var command = context.Message;

        _logger.LogDebug(
            "Consuming command {CommandType} with correlation ID {CorrelationId}",
            typeof(TCommand).Name,
            context.CorrelationId);

        try
        {
            // Bridge to mediator - command handlers remain unchanged!
            // Note: This assumes TCommand implements ICommand<TResult>
            // We need to use reflection to call Send with the correct generic type
            var commandType = command.GetType();
            var resultType = GetResultType(commandType);

            if (resultType == null)
            {
                throw new InvalidOperationException(
                    $"Command {commandType.Name} does not implement ICommand<TResult>");
            }

            var sendMethod = typeof(IMediator)
                .GetMethod(nameof(IMediator.Send))!
                .MakeGenericMethod(resultType);

            var sendTask = (Task)sendMethod.Invoke(_mediator, new object[] { command, context.CancellationToken })!;
            await sendTask;

            _logger.LogDebug(
                "Successfully consumed command {CommandType}",
                typeof(TCommand).Name);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to consume command {CommandType}",
                typeof(TCommand).Name);
            throw;
        }
    }

    private static Type? GetResultType(Type commandType)
    {
        // Find ICommand<TResult> interface
        var commandInterface = commandType
            .GetInterfaces()
            .FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ICommand<>));

        return commandInterface?.GetGenericArguments()[0];
    }
}


