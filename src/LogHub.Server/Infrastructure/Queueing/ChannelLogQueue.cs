using System.Threading.Channels;
using LogHub.Server.Options;
using Microsoft.Extensions.Options;

namespace LogHub.Server.Infrastructure.Queueing;

/// <summary>
/// Bounded in-memory queue used to decouple WebSocket ingestion from disk writes.
/// </summary>
public sealed class ChannelLogQueue
{
    private readonly Channel<QueuedLogWrite> _channel;

    /// <summary>
    /// Initializes a new queue instance from configured capacity options.
    /// </summary>
    /// <param name="options">Server options wrapper.</param>
    public ChannelLogQueue(IOptions<LogHubOptions> options)
    {
        ArgumentNullException.ThrowIfNull(options);

        int capacity = Math.Max(1, options.Value.QueueCapacity);
        // Une seule lecture en background, plusieurs ecritures depuis les clients WS.
        _channel = Channel.CreateBounded<QueuedLogWrite>(new BoundedChannelOptions(capacity)
        {
            FullMode = BoundedChannelFullMode.Wait,
            SingleReader = true,
            SingleWriter = false
        });
    }

    /// <summary>
    /// Enqueues a log write request.
    /// </summary>
    /// <param name="item">The queued write payload.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns><c>true</c> when the item is queued.</returns>
    public async ValueTask<bool> EnqueueAsync(QueuedLogWrite item, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(item);
        await _channel.Writer.WriteAsync(item, cancellationToken);
        return true;
    }

    /// <summary>
    /// Dequeues the next queued log write request.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The next queued write payload.</returns>
    public async ValueTask<QueuedLogWrite> DequeueAsync(CancellationToken cancellationToken)
        => await _channel.Reader.ReadAsync(cancellationToken);
}
