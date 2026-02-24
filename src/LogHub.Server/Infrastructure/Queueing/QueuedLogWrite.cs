namespace LogHub.Server.Infrastructure.Queueing;

/// <summary>
/// Represents a normalized log write request queued for background persistence.
/// </summary>
public sealed class QueuedLogWrite
{
    /// <summary>
    /// Gets or sets the payload extension (json/xml).
    /// </summary>
    public string Extension { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the serialized payload written to disk.
    /// </summary>
    public string SerializedEntry { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the UTC timestamp used for daily file partitioning.
    /// </summary>
    public DateTime TimestampUtc { get; set; }
}
