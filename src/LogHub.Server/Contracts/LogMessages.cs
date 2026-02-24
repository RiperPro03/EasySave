namespace LogHub.Server.Contracts;

/// <summary>
/// Represents a WebSocket request sent by an EasyLog client.
/// </summary>
public sealed class LogEnvelope
{
    /// <summary>
    /// Gets or sets the operation name ("write" or "read").
    /// </summary>
    public string Operation { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the serialized payload extension (json/xml) for write requests.
    /// </summary>
    public string? Extension { get; set; }

    /// <summary>
    /// Gets or sets the serialized log entry payload for write requests.
    /// </summary>
    public string? SerializedEntry { get; set; }

    /// <summary>
    /// Gets or sets the UTC timestamp associated with the write request.
    /// </summary>
    public DateTime? TimestampUtc { get; set; }

    /// <summary>
    /// Gets or sets the maximum number of files to read for read requests.
    /// </summary>
    public int? MaxFiles { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether all available files should be read.
    /// </summary>
    public bool ReadAll { get; set; }
}

/// <summary>
/// Represents a WebSocket response returned by the server.
/// </summary>
public sealed class LogEnvelopeResponse
{
    /// <summary>
    /// Gets or sets a value indicating whether the operation succeeded.
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Gets or sets an error message when the operation fails.
    /// </summary>
    public string? Error { get; set; }

    /// <summary>
    /// Gets or sets the returned serialized log entries for read operations.
    /// </summary>
    public List<SerializedLogEntry>? Entries { get; set; }
}

/// <summary>
/// Represents a serialized log entry transported over WebSocket.
/// </summary>
public sealed class SerializedLogEntry
{
    /// <summary>
    /// Gets or sets the serialized payload extension (json/xml).
    /// </summary>
    public string Extension { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the serialized entry content.
    /// </summary>
    public string SerializedEntry { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the source file name when known.
    /// </summary>
    public string? FileName { get; set; }
}
