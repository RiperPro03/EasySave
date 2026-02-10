namespace EasySave.Core.DTO;

/// <summary>
/// Represents a log entry (daily log).
/// Serializable data (JSON/XML).
/// </summary>
public class LogEntryDto
{
    public DateTime TimestampUtc { get; set; }

    public string JobName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the source path (file or folder, depending on implementation).
    /// </summary>
    public string SourcePath { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the target path.
    /// </summary>
    public string TargetPath { get; set; } = string.Empty;

    public long FileSizeBytes { get; set; }

    /// <summary>
    /// Gets or sets the transfer duration in milliseconds.
    /// </summary>
    public double TransferTimeMs { get; set; }

    /// <summary>
    /// Gets or sets the text status (e.g., "OK", "ERROR", "SKIPPED").
    /// </summary>
    public string Status { get; set; } = "OK";

    /// <summary>
    /// Gets or sets the error message when <see cref="Status"/> is "ERROR".
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="LogEntryDto"/> class.
    /// </summary>
    public LogEntryDto() { }

}
