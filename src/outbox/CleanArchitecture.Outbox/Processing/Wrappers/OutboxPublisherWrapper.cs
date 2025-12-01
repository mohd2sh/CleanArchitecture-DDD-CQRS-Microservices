using System;
using System.Threading;
using System.Threading.Tasks;
using CleanArchitecture.Outbox.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace CleanArchitecture.Outbox.Processing.Wrappers;


internal sealed class OutboxPublisherWrapper<TEvent> : OutboxPublisherWrapperBase
    where TEvent : class
{
    public override async Task PublishAsync(
        object integrationEvent,
        IServiceProvider serviceProvider,
        CancellationToken cancellationToken)
    {
        var typedEvent = (TEvent)integrationEvent;

        var publisher = serviceProvider.GetRequiredService<IOutboxPublisher>();

        await publisher.PublishAsync(typedEvent, cancellationToken);
    }
}

