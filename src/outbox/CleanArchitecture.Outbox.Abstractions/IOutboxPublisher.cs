using System.Threading;
using System.Threading.Tasks;

namespace CleanArchitecture.Outbox.Abstractions;

/// <summary>
/// Interface for publishing integration events from outbox to message bus.
/// Abstracts the message bus implementation (MassTransit, etc.)
/// </summary>
public interface IOutboxPublisher
{
    /// <summary>
    /// Publishes an integration event to the message bus.
    /// </summary>
    /// <typeparam name="TEvent">The type of integration event.</typeparam>
    /// <param name="integrationEvent">The integration event to publish.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Task representing the async operation.</returns>
    Task PublishAsync<TEvent>(TEvent integrationEvent, CancellationToken cancellationToken = default)
        where TEvent : class;
}





