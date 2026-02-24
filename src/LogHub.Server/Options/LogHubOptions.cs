namespace LogHub.Server.Options;

/// <summary>
/// Configuration options for the LogHub server runtime.
/// </summary>
public sealed class LogHubOptions
{
    /// <summary>
    /// Configuration section name used for binding.
    /// </summary>
    public const string SectionName = "LogHub";

    /// <summary>
    /// Gets or sets the TCP port used by Kestrel.
    /// </summary>
    public int Port { get; set; } = 9696;

    /// <summary>
    /// Gets or sets the WebSocket endpoint path.
    /// </summary>
    public string WebSocketPath { get; set; } = "/ws/logs";

    /// <summary>
    /// Gets or sets the local directory used to persist logs.
    /// </summary>
    public string LogDirectory { get; set; } = "logs";

    /// <summary>
    /// Gets or sets the bounded in-memory queue capacity.
    /// </summary>
    public int QueueCapacity { get; set; } = 4096;
}
