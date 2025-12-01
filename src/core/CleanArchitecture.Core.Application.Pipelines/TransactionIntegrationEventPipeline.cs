using CleanArchitecture.Core.Application.Abstractions.Events;
using CleanArchitecture.Core.Application.Abstractions.Messaging;
using CleanArchitecture.Core.Application.Abstractions.Persistence;
using Microsoft.Extensions.Logging;

namespace CleanArchitecture.Core.Application.Pipelines;

public sealed class TransactionIntegrationEventPipeline<TEvent> : IIntegrationEventPipeline<TEvent>
{
    private readonly IUnitOfWork _uow;
    private readonly ILogger<TransactionIntegrationEventPipeline<TEvent>> _logger;

    public TransactionIntegrationEventPipeline(
        IUnitOfWork uow,
        ILogger<TransactionIntegrationEventPipeline<TEvent>> logger)
    {
        _uow = uow;
        _logger = logger;
    }

    public async Task Handle(TEvent @event, PipelineDelegate next, CancellationToken cancellationToken = default)
    {
        await using var transaction = await _uow.BeginTransactionAsync(cancellationToken);

        try
        {
            await next();

            await _uow.SaveChangesAsync(cancellationToken);

            await transaction.CommitAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Transaction failed for integration event {EventType}", typeof(TEvent).Name);

            await transaction.RollbackAsync(cancellationToken);

            throw;
        }
    }
}

