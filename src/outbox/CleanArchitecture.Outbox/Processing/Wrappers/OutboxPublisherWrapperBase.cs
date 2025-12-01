using System;
using System.Threading;
using System.Threading.Tasks;

namespace CleanArchitecture.Outbox.Processing.Wrappers;

/// <summary>
/// Base class for outbox publisher wrappers. Allows OutboxMessageProcessor to work with
/// wrappers without knowing the concrete event type at compile time.
/// </summary>
internal abstract class OutboxPublisherWrapperBase
{
    public abstract Task PublishAsync(
        object integrationEvent,
        IServiceProvider serviceProvider,
        CancellationToken cancellationToken);
}

