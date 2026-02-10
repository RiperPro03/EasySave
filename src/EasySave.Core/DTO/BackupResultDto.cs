namespace EasySave.Core.DTO;

/// <summary>
/// Represents the result of a job execution (useful for CLI/GUI/tests).
/// </summary>
public class BackupResultDto
{
    /// <summary>
    /// Gets or sets a value indicating whether the run succeeded.
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Gets or sets a human-readable message for the result.
    /// </summary>
    public string Message { get; set; } = string.Empty;

    public int FilesProcessed { get; set; }
    public long TotalBytesProcessed { get; set; }
    public int CopiedCount { get; set; }
    public int SkippedCount { get; set; }
    public int ErrorCount { get; set; }

    /// <summary>
    /// Gets or sets the total duration of the run.
    /// </summary>
    public TimeSpan Duration { get; set; }

    public List<string> Errors { get; set; } = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="BackupResultDto"/> class.
    /// </summary>
    public BackupResultDto() { }
}
