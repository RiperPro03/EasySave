using LogHub.Server.Infrastructure.Queueing;
using LogHub.Server.Infrastructure.Storage;

namespace LogHub.Server.Workers;

/// <summary>
/// Background worker that drains the in-memory queue and persists log entries to storage.
/// </summary>
public sealed class LogIngestWorker : BackgroundService
{
    private readonly ChannelLogQueue _queue;
    private readonly DailyFileLogWriter _writer;
    private readonly ILogger<LogIngestWorker> _logger;

    /// <summary>
    /// Initializes a new worker instance.
    /// </summary>
    /// <param name="queue">Queue source for pending writes.</param>
    /// <param name="writer">Storage service used to persist entries.</param>
    /// <param name="logger">Framework logger.</param>
    public LogIngestWorker(ChannelLogQueue queue, DailyFileLogWriter writer, ILogger<LogIngestWorker> logger)
    {
        _queue = queue;
        _writer = writer;
        _logger = logger;
    }

    /// <summary>
    /// Runs the background dequeue/write loop until shutdown.
    /// </summary>
    /// <param name="stoppingToken">Cancellation token for graceful shutdown.</param>
    /// <returns>A task representing the worker lifetime.</returns>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                // Consomme les ecritures une par une pour conserver l'ordre d'ingestion.
                var item = await _queue.DequeueAsync(stoppingToken);
                await _writer.WriteAsync(item, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while ingesting queued log entry.");
            }
        }
    }
}
